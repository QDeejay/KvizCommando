using KvizCommando.Server.Hubs;
using KvizCommando.Server.Services.VsGame.Matchmaking;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.SignalR;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed partial class VsMatchService : IVsMatchService
{
    private readonly VsMatchStore _store;
    private readonly VsMatchSetupService _setup;
    private readonly VsMatchRewardPersistenceService _rewardPersistence;
    private readonly IHubContext<VsMatchHub, IVsMatchHubClient> _hub;
    private readonly ILogger<VsMatchService> _logger;

    public VsMatchService(
        VsMatchStore store,
        VsMatchSetupService setup,
        VsMatchRewardPersistenceService rewardPersistence,
        IHubContext<VsMatchHub, IVsMatchHubClient> hub,
        ILogger<VsMatchService> logger)
    {
        _store = store;
        _setup = setup;
        _rewardPersistence = rewardPersistence;
        _hub = hub;
        _logger = logger;
    }

    /// <inheritdoc />
    public VsMatchSession LockMatch(
        IReadOnlyList<VsRankedQueueEntry> entries)
    {
        if (entries.Count < VsRankedQueueRules.MINIMUM_PLAYERS ||
            entries.Count > VsRankedQueueRules.MAXIMUM_PLAYERS)
        {
            throw new InvalidOperationException(
                "A ranked match must be locked with a valid player count.");
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

    /// <inheritdoc />
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
                        player.IsConnected || player.IsBot);

                if (!hasConnectedPlayer)
                {
                    messages = [];
                }
                else
                {
                    StartPhaseLocked(
                        match,
                        VsMatchPhase.PreparationStarting);

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
            ResponseTimeMilliseconds =
                entry.ResponseTimeMilliseconds,
            ConnectionQuality = entry.ConnectionQuality,
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

    private static void AddLog( VsMatchSession match, int? playerId, string eventType, string data)
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
