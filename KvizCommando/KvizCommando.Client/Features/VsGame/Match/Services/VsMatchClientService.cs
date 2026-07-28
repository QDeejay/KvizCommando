using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Contracts.VsGame.Match;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace KvizCommando.Client.Features.VsGame.Match.Services;

public sealed class VsMatchClientService : IVsMatchClientService
{
    private readonly NavigationManager _navigation;
    private readonly SessionService _session;
    private readonly ILogger<VsMatchClientService> _logger;
    private readonly List<IDisposable> _handlers = [];

    private HubConnection? _connection;

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
    public string ErrorMessageKey { get; private set; } = string.Empty;
    public bool IsConnected =>
        _connection?.State == HubConnectionState.Connected;

    public async Task StartAsync(
        int classificationId,
        CancellationToken ct = default)
    {
        await StopAsync(ct);

        QueueSnapshot = null;
        MatchSnapshot = null;
        ErrorMessageKey = string.Empty;

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
            "CommandRejected",
            messageKey =>
            {
                ErrorMessageKey = messageKey;
                NotifyChanged();
            }));

        _connection.Closed += HandleClosedAsync;

        await _connection.StartAsync(ct);

        await _connection.InvokeAsync(
            "JoinRankedQueue",
            classificationId,
            _session.SessionId ?? "NoId",
            ct);
    }

    public Task LeaveQueueAsync(CancellationToken ct = default) =>
        InvokeAsync("LeaveRankedQueue", ct);

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
        return _connection?.State == HubConnectionState.Connected
            ? _connection.InvokeAsync(methodName, ct)
            : Task.CompletedTask;
    }

    private Task InvokeAsync<T>(
        string methodName,
        T argument,
        CancellationToken ct)
    {
        return _connection?.State == HubConnectionState.Connected
            ? _connection.InvokeAsync(
                methodName,
                argument,
                ct)
            : Task.CompletedTask;
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
 * Egyetlen, automatikusan újra nem kapcsolódó SignalR kapcsolatot
 * kezel, fogadja a queue/match snapshotokat és továbbítja a
 * preparációs parancsokat.
 */
