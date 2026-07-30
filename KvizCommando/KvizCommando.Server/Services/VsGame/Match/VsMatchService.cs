using KvizCommando.Server.Hubs;
using KvizCommando.Server.Services.VsGame.Matchmaking;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.SignalR;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed class VsMatchService : IVsMatchService
{
    private readonly VsMatchStore _store;
    private readonly VsMatchSetupService _setup;
    private readonly IHubContext<VsMatchHub, IVsMatchHubClient> _hub;
    private readonly ILogger<VsMatchService> _logger;

    public VsMatchService(
        VsMatchStore store,
        VsMatchSetupService setup,
        IHubContext<VsMatchHub, IVsMatchHubClient> hub,
        ILogger<VsMatchService> logger)
    {
        _store = store;
        _setup = setup;
        _hub = hub;
        _logger = logger;
    }

    public VsMatchSession LockMatch(
        IReadOnlyList<VsRankedQueueEntry> entries)
    {
        if (entries.Count != VsMatchProfiles.Ranked.RequiredPlayers)
        {
            throw new InvalidOperationException(
                "A ranked match must be locked with the configured player count.");
        }

        var classificationId = entries[0].ClassificationId;

        if (entries.Any(entry =>
                entry.ClassificationId != classificationId))
        {
            throw new InvalidOperationException(
                "A ranked match cannot contain multiple classifications.");
        }

        var classification =
            VsBattleClassificationRules.List.First(rule =>
                rule.ClassificationId == classificationId);

        var match = new VsMatchSession
        {
            Profile = VsMatchProfiles.Ranked,
            Classification = classification,
            Players =
            [
                .. entries.Select((entry, index) =>
                    CreateLockedPlayer(
                        entry,
                        index + 1,
                        classification.RequiredPartySize))
            ]
        };

        AddLog(match, null, "MatchLocked",
            $"Classification={classificationId}");

        if (!_store.TryAdd(match))
        {
            match.Dispose();
            throw new InvalidOperationException(
                "The locked VS match could not be registered.");
        }

        return match;
    }

    public async Task<bool> InitializeLockedMatchAsync(
        VsMatchSession match,
        CancellationToken ct = default)
    {
        try
        {
            await BroadcastMatchAsync(match);

            var hasConnectedPlayer =
                await _setup.InitializePlayersAsync(match, ct);

            (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

            lock (match.SyncRoot)
            {
                if (match.IsClosed)
                    return false;

                match.IsInitializing = false;
                hasConnectedPlayer =
                    hasConnectedPlayer &&
                    match.Players.Any(player =>
                        player.IsConnected);

                if (!hasConnectedPlayer)
                {
                    messages = [];
                }
                else
                {
                    StartPhaseLocked(
                        match,
                        VsMatchPhase.PreparationOrder);

                    messages =
                        VsMatchSnapshotBuilder.BuildMessages(match);
                }
            }

            if (!hasConnectedPlayer)
            {
                _store.TryRemove(match.MatchId, out _);
                return false;
            }

            await SendBroadcastMessagesAsync(messages);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "VS match initialization failed. matchId={MatchId}",
                match.MatchId);

            string[] connectedClients;

            lock (match.SyncRoot)
            {
                match.IsInitializing = false;
                connectedClients =
                [
                    .. match.Players
                        .Where(player => player.IsConnected)
                        .Select(player => player.ConnectionId)
                ];
            }

            await _setup.RefundStakesAsync(match);
            _store.TryRemove(match.MatchId, out _);

            await SendMatchClosedAsync(
                connectedClients,
                "vsgame.Match.Error.Connection");

            return false;
        }
    }

    public async Task SelectCharacterAsync(
        string connectionId,
        int slotNumber,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);
            var target = VsMatchPreparationRules.SelectCharacter(
                match,
                player,
                slotNumber);

            if (target is null)
                return;

            AddLog(
                match,
                player!.PlayerId,
                "PreparationCharacterSelected",
                $"Round={target.RoundNumber};Slot={slotNumber}");

            messages = VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task AssignLoadoutAsync(
        string connectionId,
        VsLoadoutAssignmentRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);
            var target = VsMatchPreparationRules.AssignLoadout(
                match,
                player,
                request.LoadoutPosition,
                request.RoundNumber);

            if (target is null)
                return;

            AddLog(
                match,
                player!.PlayerId,
                "PreparationLoadoutAssigned",
                $"Round={target.RoundNumber};" +
                $"Position={request.LoadoutPosition}");

            messages = VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task AssignHelpAsync(
        string connectionId,
        VsHelpAssignmentRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);
            var target = VsMatchPreparationRules.AssignHelp(
                match,
                player,
                request.HelpType,
                request.RoundNumber);

            if (target is null)
                return;

            AddLog(
                match,
                player!.PlayerId,
                "PreparationHelpAssigned",
                $"Round={target.RoundNumber};Help={request.HelpType}");

            messages = VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task ResetPreparationAsync(
        string connectionId,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);

            if (!VsMatchPreparationRules.Reset(match, player))
                return;

            AddLog(
                match,
                player!.PlayerId,
                "PreparationReset",
                string.Empty);

            messages = VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task FinishPreparationAsync(
        string connectionId,
        CancellationToken ct = default)
    {
        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        ct.ThrowIfCancellationRequested();

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);

            if (!VsMatchPreparationRules.Finish(match, player))
                return;

            AddLog(
                match,
                player!.PlayerId,
                "PreparationFinished",
                string.Empty);

            AdvanceIfReadyLocked(match);

            messages = VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task DisconnectAsync(
        string connectionId,
        CancellationToken ct = default)
    {
        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        ct.ThrowIfCancellationRequested();

        var removeMatch = false;
        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages = [];

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);

            if (match.IsClosed ||
                player is null ||
                !player.IsConnected)
            {
                return;
            }

            player.IsConnected = false;
            VsMatchPreparationRules.ApplyTimeoutDefaults(
                match,
                player);
            player.IsFinished = true;

            AddLog(
                match,
                player.PlayerId,
                "Disconnected",
                string.Empty);

            AdvanceIfReadyLocked(match);

            removeMatch =
                !match.IsInitializing &&
                match.Players.All(item => !item.IsConnected);

            if (!removeMatch &&
                match.Players.Any(item => item.IsConnected))
            {
                messages =
                    VsMatchSnapshotBuilder.BuildMessages(match);
            }
        }

        if (removeMatch)
        {
            _store.TryRemove(match.MatchId, out _);
            return;
        }

        await SendBroadcastMessagesAsync(messages);
    }

    private static VsMatchPlayerState CreateLockedPlayer(
        VsRankedQueueEntry entry,
        int position,
        int teamSize) =>
        new()
        {
            PlayerId = entry.PlayerId,
            Position = position,
            SessionId = entry.SessionId,
            ConnectionId = entry.ConnectionId,
            DisplayName = entry.DisplayName,
            TeamName = entry.TeamName,
            TeamLevel = entry.TeamLevel,
            Rounds =
            [
                .. Enumerable.Range(1, teamSize + 1)
                    .Select(roundNumber => new VsMatchRoundState
                    {
                        RoundNumber = roundNumber,
                        IsCaptainRound = roundNumber == teamSize + 1
                    })
            ]
        };

    private void StartPhaseLocked(
        VsMatchSession match,
        VsMatchPhase phase)
    {
        match.PhaseTimerCts.Cancel();
        match.PhaseTimerCts.Dispose();
        match.PhaseTimerCts = new CancellationTokenSource();
        match.Phase = phase;
        VsMatchPreparationRules.BeginPhase(match, phase);

        if (phase == VsMatchPhase.PreparationCompleted)
        {
            match.DeadlineUtc = null;
            AddLog(match, null, "PreparationCompleted", string.Empty);
            return;
        }

        if (phase == VsMatchPhase.PreparationHelps &&
            match.Players.All(player => player.IsFinished))
        {
            StartPhaseLocked(
                match,
                VsMatchPhase.PreparationCompleted);
            return;
        }

        match.DeadlineUtc = DateTime.UtcNow.AddSeconds(
            match.Profile.PreparationSeconds);

        AddLog(match, null, "PhaseStarted", phase.ToString());

        _ = RunPhaseTimerAsync(
            match.MatchId,
            match.DeadlineUtc.Value,
            match.PhaseTimerCts.Token);
    }

    private async Task RunPhaseTimerAsync(
        Guid matchId,
        DateTime deadlineUtc,
        CancellationToken ct)
    {
        try
        {
            var delay = deadlineUtc - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, ct);

            if (ct.IsCancellationRequested)
                return;

            if (!_store.TryGet(matchId, out var match) ||
                match is null)
            {
                return;
            }

            (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

            lock (match.SyncRoot)
            {
                if (ct.IsCancellationRequested ||
                    match.IsClosed)
                    return;

                if (match.Profile.PausePreparationOnTimeout)
                {
                    AddLog(
                        match,
                        null,
                        "PreparationTimerPaused",
                        match.Phase.ToString());
                    return;
                }

                foreach (var player in match.Players
                             .Where(player => !player.IsFinished))
                {
                    VsMatchPreparationRules.ApplyTimeoutDefaults(
                        match,
                        player);
                    player.IsFinished = true;
                    AddLog(
                        match,
                        player.PlayerId,
                        "PreparationTimeout",
                        match.Phase.ToString());
                }

                AdvanceIfReadyLocked(match);

                messages =
                    VsMatchSnapshotBuilder.BuildMessages(match);
            }

            await SendBroadcastMessagesAsync(messages);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "VS phase timer failed. matchId={MatchId}",
                matchId);
        }
    }

    private void AdvanceIfReadyLocked(VsMatchSession match)
    {
        var nextPhase =
            VsMatchPreparationRules.GetNextPhase(match);

        if (nextPhase.HasValue)
            StartPhaseLocked(match, nextPhase.Value);
    }

    private async Task BroadcastMatchAsync(
        VsMatchSession match)
    {
        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            if (match.IsClosed)
                return;

            messages =
                VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    private async Task SendBroadcastMessagesAsync(
        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages)
    {
        foreach (var message in messages)
        {
            await _hub.Clients
                .Client(message.ConnectionId)
                .MatchChanged(message.Snapshot);
        }
    }

    private async Task SendMatchClosedAsync(
        IEnumerable<string> connectionIds,
        string messageKey)
    {
        foreach (var connectionId in connectionIds)
        {
            await _hub.Clients
                .Client(connectionId)
                .MatchClosed(messageKey);
        }
    }

    private static void AddLog(
        VsMatchSession match,
        int? playerId,
        string eventType,
        string data)
    {
        match.EventLog.Add(new VsMatchEventLogEntry
        {
            Phase = match.Phase,
            PlayerId = playerId,
            EventType = eventType,
            Data = data
        });
    }
}

/**
 * MÓDOSÍTÁS: a preparáció feltételei és állapotmódosításai a
 * VsMatchPreparationRules osztályba kerültek. A service publikus
 * műveletei rövid, explicit koordinátorok maradtak: sessionkeresés,
 * lock, naplózás, fázisváltás és snapshot-küldés. A fázis időzítőjét
 * kizárólag a saját CancellationTokenje érvényteleníti; technikai
 * PhaseVersion és az egyszer használatos ToQueue segéd megszűnt.
 * Az in-memory állapot csak await nélküli lock alatt változik, a
 * SignalR-küldés a lockon kívül történik.
 *
 * A fájl a MatchLocked session létrehozását, a preparációs parancsok
 * authoritative feldolgozását, a fázisváltást, a szerverórát és a
 * disconnect miatti takarítást koordinálja.
 */
