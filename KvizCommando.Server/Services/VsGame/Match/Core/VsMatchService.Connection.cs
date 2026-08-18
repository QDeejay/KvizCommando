using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed partial class VsMatchService
{
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
        var releasePlayer = false;
        VsMatchRewardState? abandonedReward = null;
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

            if (match.Phase == VsMatchPhase.GameCompleted)
            {
                player.IsConnected = false;
                releasePlayer = true;
                removeMatch = match.Players.All(item => !item.IsConnected);
            }
            else
            {
                VsMatchBotRules.Activate(match, player);

                if (IsPreparationPhase(match.Phase))
                {
                    VsMatchPreparationRules.ApplyTimeoutDefaults(
                        match,
                        player);
                }

                player.IsFinished = true;
            }

            AddLog(
                match,
                player.PlayerId,
                "Disconnected",
                string.Empty);

            if (!releasePlayer &&
                match.Players.All(item => item.IsBot))
            {
                match.IsClosed = true;
                match.PhaseTimerCts.Cancel();
                abandonedReward =
                    VsMatchRewardCalculator.Calculate(match);
                match.Reward = abandonedReward;
                removeMatch = true;

                AddLog(
                    match,
                    null,
                    "AllPlayersDisconnected",
                    string.Empty);
            }
            else if (!releasePlayer && IsPreparationPhase(match.Phase))
            {
                AdvanceIfReadyLocked(match);
            }
            else if (!releasePlayer && IsAnswerPhase(match.Phase) &&
                     VsMatchGameRules
                         .HaveAllParticipantsAnswered(match))
            {
                StartAnswerResultDelayLocked(match);
            }
            else if (!releasePlayer && IsAnswerPhase(match.Phase))
            {
                ScheduleBotAnswerLocked(match, player);
            }

            if (!releasePlayer &&
                match.Players.Any(item => item.IsConnected))
            {
                messages =
                    VsMatchSnapshotBuilder.BuildMessages(match);
            }
        }

        if (releasePlayer)
            _store.ReleasePlayer(match, match.Players.First(player =>
                player.ConnectionId == connectionId));

        if (abandonedReward is not null)
        {
            await _rewardPersistence.SaveAsync(
                match.MatchId,
                match.Players.Count,
                abandonedReward);
        }

        if (removeMatch)
        {
            _store.TryRemove(match.MatchId, out _);
            return;
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public Task DisconnectPlayerAsync(
        int playerId,
        CancellationToken ct = default)
    {
        if (!_store.TryGetByPlayer(playerId, out var match) ||
            match is null)
        {
            return Task.CompletedTask;
        }

        string? connectionId;

        lock (match.SyncRoot)
        {
            connectionId = match.Players.FirstOrDefault(player =>
                player.PlayerId == playerId)?.ConnectionId;
        }

        return connectionId is null
            ? Task.CompletedTask
            : DisconnectAsync(connectionId, ct);
    }
}
