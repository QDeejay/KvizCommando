using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace KvizCommando.Client.Features.Solo.Services;

public sealed class SoloGameClientService : ISoloGameClientService
{
    private readonly NavigationManager _navigation;
    private readonly SessionService _session;
    private readonly ILogger<SoloGameClientService> _logger;
    private readonly List<IDisposable> _handlers = [];

    private HubConnection? _connection;

    public SoloGameClientService(
        NavigationManager navigation,
        SessionService session,
        ILogger<SoloGameClientService> logger)
    {
        _navigation = navigation;
        _session = session;
        _logger = logger;
    }

    public event Action? OnChanged;

    public VsConnectionCheckResult? ConnectionCheck { get; private set; }
    public string ErrorMessageKey { get; private set; } = string.Empty;
    public bool IsConnected =>
        _connection?.State == HubConnectionState.Connected;

    /// <summary>
    /// Létrehozza a SignalR-kapcsolatot, ellenőrzi annak minőségét, majd elindítja az egyéni játékot.
    /// </summary>
    public async Task<StartSoloGameResponse?> StartAsync(
        StartSoloGameRequest request,
        CancellationToken ct = default)
    {
        await StopAsync(ct);

        ErrorMessageKey = string.Empty;
        NotifyChanged();

        var sessionId = _session.SessionId;
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            ErrorMessageKey = "solo.Error.Identity";
            return null;
        }

        request.SessionId = sessionId;
        _connection = new HubConnectionBuilder()
            .WithUrl(_navigation.ToAbsoluteUri("/hubs/solo-game"))
            .Build();

        _handlers.Add(_connection.On<long, long>(
            "LatencyProbe",
            token => Task.FromResult(token)));
        _connection.Closed += HandleClosedAsync;

        try
        {
            await _connection.StartAsync(ct);

            ConnectionCheck = await _connection
                .InvokeAsync<VsConnectionCheckResult>(
                    "CheckConnection",
                    ct);
            NotifyChanged();
            await Task.Delay(
                TimeSpan.FromSeconds(1),
                ct);

            if (ConnectionCheck.Quality ==
                VsConnectionQuality.Bad)
            {
                ErrorMessageKey =
                    "solo.Error.ConnectionSpeed";
                await StopAsync(CancellationToken.None);
                return null;
            }

            var result = await _connection
                .InvokeAsync<StartSoloHubResponse>(
                    "StartSoloGame",
                    request,
                    ct);

            if (!result.IsAccepted || result.Game is null)
            {
                ErrorMessageKey = result.ErrorKey;
                await StopAsync(CancellationToken.None);
                return null;
            }

            return result.Game;
        }
        catch (OperationCanceledException) when (
            ct.IsCancellationRequested)
        {
            await StopAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to start Solo SignalR game.");
            ErrorMessageKey = "solo.Error.Connection";
            await StopAsync(CancellationToken.None);
            return null;
        }
    }

    /// <summary>
    /// Kiértékelésre beküldi az egyéni játék válaszát.
    /// </summary>
    public async Task<SoloHubAnswerResponse?> SubmitAnswerAsync(
        SoloAnswerDto answer,
        CancellationToken ct = default)
    {
        if (!IsConnected)
            return null;

        try
        {
            var response = await _connection!
                .InvokeAsync<SoloHubAnswerResponse>(
                    "SubmitAnswer",
                    answer,
                    ct);

            if (!response.IsAccepted)
            {
                ErrorMessageKey = response.ErrorKey;
                NotifyChanged();
                return response;
            }

            ConnectionCheck = response.Connection;
            NotifyChanged();
            return response;
        }
        catch (OperationCanceledException) when (
            ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to submit Solo answer.");
            ErrorMessageKey = "solo.Error.Connection";
            NotifyChanged();
            return null;
        }
    }

    /// <summary>
    /// Megszakítja az aktuális egyéni játékot.
    /// </summary>
    public async Task<bool> AbandonAsync(
        CancellationToken ct = default)
    {
        if (!IsConnected)
            return false;

        try
        {
            return await _connection!
                .InvokeAsync<bool>(
                    "AbortSoloGame",
                    ct);
        }
        catch (OperationCanceledException) when (
            ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(
                ex,
                "Solo game connection closed before abort.");
            return false;
        }
    }

    /// <summary>
    /// Leállítja az aktuális játékkapcsolatot.
    /// </summary>
    public async Task StopAsync(
        CancellationToken ct = default)
    {
        var connection = _connection;
        ConnectionCheck = null;

        if (connection is null)
            return;

        _connection = null;
        connection.Closed -= HandleClosedAsync;

        foreach (var handler in _handlers)
            handler.Dispose();

        _handlers.Clear();

        try
        {
            await connection.StopAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(
                ex,
                "Solo SignalR connection was already closed.");
        }

        await connection.DisposeAsync();
    }

    private Task HandleClosedAsync(Exception? exception)
    {
        if (exception is not null)
        {
            _logger.LogWarning(
                exception,
                "Solo SignalR connection closed.");
        }

        ErrorMessageKey = "solo.Error.Connection";
        ConnectionCheck = ConnectionCheck is null
            ? null
            : new VsConnectionCheckResult
            {
                ResponseTimeMilliseconds =
                    ConnectionCheck.ResponseTimeMilliseconds,
                Quality = VsConnectionQuality.Unknown
            };
        NotifyChanged();
        return Task.CompletedTask;
    }

    private void NotifyChanged() => OnChanged?.Invoke();

    /// <summary>
    /// Aszinkron módon felszabadítja a példány által használt erőforrásokat.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        GC.SuppressFinalize(this);
    }
}
