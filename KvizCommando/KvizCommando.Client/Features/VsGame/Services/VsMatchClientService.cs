using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

namespace KvizCommando.Client.Features.VsGame.Services;

public sealed class VsMatchClientService : IVsMatchClientService
{
    private readonly NavigationManager _navigation;
    private readonly SessionService _session;
    private readonly ILogger<VsMatchClientService> _logger;
    private readonly List<IDisposable> _handlers = [];
    private readonly Stopwatch _serverClock = new();

    private HubConnection? _connection;
    private DateTime _serverUtcAtSync;

    public VsMatchClientService(
        NavigationManager navigation,
        SessionService session,
        ILogger<VsMatchClientService> logger)
    {
        _navigation = navigation;
        _session = session;
        _logger = logger;
    }

    public event Action? OnChanged;

    public VsRankedQueueSnapshot? QueueSnapshot { get; private set; }
    public VsMatchSnapshot? MatchSnapshot { get; private set; }
    public VsConnectionCheckResult? ConnectionCheck { get; private set; }
    public string ErrorMessageKey { get; private set; } = string.Empty;
    public bool IsConnected =>
        _connection?.State == HubConnectionState.Connected;
    public DateTime ServerUtcNow =>
        _serverUtcAtSync + _serverClock.Elapsed;

    public async Task<VsQueueJoinResult> StartAsync(
        int classificationId,
        CancellationToken ct = default)
    {
        await StopAsync(ct);

        QueueSnapshot = null;
        MatchSnapshot = null;
        ConnectionCheck = null;
        ErrorMessageKey = string.Empty;

        var sessionId = _session.SessionId;
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return new VsQueueJoinResult
            {
                ErrorKey = "vsgame.Match.Error.Identity"
            };
        }

        _connection = new HubConnectionBuilder()
            .WithUrl(_navigation.ToAbsoluteUri("/hubs/vs-match"))
            .Build();

        _handlers.Add(_connection.On<VsRankedQueueSnapshot>(
            "QueueChanged",
            snapshot =>
            {
                QueueSnapshot = snapshot;
                NotifyChanged();
            }));

        _handlers.Add(_connection.On<VsMatchSnapshot>(
            "MatchChanged",
            snapshot =>
            {
                MatchSnapshot = snapshot;
                QueueSnapshot = null;
                NotifyChanged();
            }));

        _handlers.Add(_connection.On<string>(
            "MatchClosed",
            messageKey =>
            {
                ErrorMessageKey = messageKey;
                QueueSnapshot = null;
                MatchSnapshot = null;
                NotifyChanged();
            }));

        _handlers.Add(_connection.On<long, long>(
            "LatencyProbe",
            token => Task.FromResult(token)));

        _connection.Closed += HandleClosedAsync;

        try
        {
            await _connection.StartAsync(ct);
            await SynchronizeServerClockAsync(ct);

            ConnectionCheck =
                await _connection
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
                var rejected = new VsQueueJoinResult
                {
                    ErrorKey =
                        "vsgame.Match.Error.ConnectionSpeed"
                };

                await StopAsync(CancellationToken.None);
                return rejected;
            }

            var result =
                await _connection.InvokeAsync<VsQueueJoinResult>(
                    "JoinRankedQueue",
                    classificationId,
                    sessionId,
                    ct);

            if (!result.IsAccepted)
                await StopAsync(CancellationToken.None);

            return result;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            await StopAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "VS SignalR connection or queue join failed.");

            await StopAsync(CancellationToken.None);
            throw;
        }
    }

    public Task LeaveQueueAsync(CancellationToken ct = default) =>
        IsConnected
            ? _connection!.InvokeAsync("LeaveRankedQueue", ct)
            : Task.CompletedTask;

    public Task SelectCharacterAsync(
        int slotNumber,
        CancellationToken ct = default) =>
        InvokeAsync("SelectCharacter", slotNumber, ct);

    public Task AssignLoadoutAsync(
        VsLoadoutAssignmentRequest request,
        CancellationToken ct = default) =>
        InvokeAsync("AssignLoadout", request, ct);

    public Task AssignHelpAsync(
        VsHelpAssignmentRequest request,
        CancellationToken ct = default) =>
        InvokeAsync("AssignHelp", request, ct);

    public Task ResetPreparationAsync(CancellationToken ct = default) =>
        InvokeAsync("ResetPreparation", ct);

    public Task FinishPreparationAsync(CancellationToken ct = default) =>
        InvokeAsync("FinishPreparation", ct);

    public Task SubmitGuessAsync(
        VsGuessAnswerRequest request,
        CancellationToken ct = default) =>
        InvokeAsync("SubmitGuess", request, ct);

    public Task SubmitChoiceAsync(
        VsChoiceAnswerRequest request,
        CancellationToken ct = default) =>
        InvokeAsync("SubmitChoice", request, ct);

    public Task UseHelpAsync(
        VsUseHelpRequest request,
        CancellationToken ct = default) =>
        InvokeAsync("UseHelp", request, ct);

    public Task SelectCaptainQuestionAsync(
        VsCaptainQuestionRequest request,
        CancellationToken ct = default) =>
        InvokeAsync("SelectCaptainQuestion", request, ct);

    public async Task StopAsync(CancellationToken ct = default)
    {
        var connection = _connection;
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
                "VS SignalR connection was already closed.");
        }

        await connection.DisposeAsync();
    }

    private Task InvokeAsync(
        string methodName,
        CancellationToken ct)
    {
        return GetConnectedConnection()
            .InvokeAsync(methodName, ct);
    }

    private Task InvokeAsync<T>(
        string methodName,
        T argument,
        CancellationToken ct)
    {
        return GetConnectedConnection().InvokeAsync(
            methodName,
            argument,
            ct);
    }

    private HubConnection GetConnectedConnection() =>
        IsConnected
            ? _connection!
            : throw new InvalidOperationException(
                "The VS SignalR connection is not active.");

    private async Task SynchronizeServerClockAsync(
        CancellationToken ct)
    {
        var roundTrip = Stopwatch.StartNew();
        var serverUtc = await GetConnectedConnection()
            .InvokeAsync<DateTime>("GetServerUtc", ct);
        roundTrip.Stop();

        _serverUtcAtSync = serverUtc.AddTicks(
            roundTrip.Elapsed.Ticks / 2);
        _serverClock.Restart();
    }

    private Task HandleClosedAsync(Exception? exception)
    {
        if (exception is not null)
        {
            _logger.LogWarning(
                exception,
                "VS SignalR connection closed.");
        }

        ErrorMessageKey = MatchSnapshot is null
            ? "vsgame.Match.Error.Connection"
            : "vsgame.Match.Error.Disconnected";

        NotifyChanged();
        return Task.CompletedTask;
    }

    private void NotifyChanged() => OnChanged?.Invoke();

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        GC.SuppressFinalize(this);
    }
}

/**
 * MÓDOSÍTÁS: a queue-belépés közvetlen eredményét visszaadja, hiányzó
 * sessionnel nem nyit kapcsolatot, a megszakított csatlakozást
 * csendesen takarítja, és a játékmeneti parancsokat nem jelenti
 * sikeresnek megszakadt kapcsolat mellett. A MatchClosed esemény
 * kizárólag aszinkron, több játékost érintő meccslezárást közvetít.
 *
 * MÓDOSÍTÁS: külön metódusokban továbbítja a játékmeneti
 * szándékokat, köztük a segítség használatát; további technikai
 * azonosítót nem ad hozzájuk.
 * MÓDOSÍTÁS: kapcsolódáskor egyetlen SignalR-kéréssel megméri a
 * szerver UTC-idejét és a válaszút felével korrigálja. Ezután
 * Stopwatch alapján szolgáltatja az időt, ezért a kliens rendszerórája
 * és annak későbbi módosítása nem tolja el a visszaszámlálókat.
 * MÓDOSÍTÁS: az öt szerveroldali SignalR-próbához visszaadja a kapott
 * tokent, majd a mérés eredményét legalább egy másodpercig megmutatja
 * a queue-belépés előtt. Rossz minősítésnél nem küld belépési parancsot.
 *
 * Egyetlen, automatikusan újra nem kapcsolódó SignalR kapcsolatot
 * kezel, fogadja a queue/match snapshotokat és továbbítja a
 * preparációs és játékmeneti parancsokat.
 */
