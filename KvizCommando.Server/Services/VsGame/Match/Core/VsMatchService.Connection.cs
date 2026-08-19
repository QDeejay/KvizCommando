using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed partial class VsMatchService
{
    /// <inheritdoc />
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

        DisconnectResult? result;

        lock (match.SyncRoot)
        {
            result = ApplyDisconnectLocked(match, connectionId);
        }

        if (result is null)
            return;

        if (result.ReleasePlayer)
            _store.ReleasePlayer(match, result.Player);

        var abandonedReward = result.AbandonedReward;
        if (abandonedReward is not null)
        {
            await _rewardPersistence.SaveAsync(
                match.MatchId,
                match.Players.Count,
                abandonedReward);
        }

        if (result.RemoveMatch)
        {
            _store.TryRemove(match.MatchId, out _);
            return;
        }

        await SendBroadcastMessagesAsync(result.Messages);
    }

    private DisconnectResult? ApplyDisconnectLocked(
        VsMatchSession match,
        string connectionId)
    {
        var player = match.FindByConnection(connectionId);

        if (match.IsClosed ||
            player is null ||
            !player.IsConnected)
        {
            return null;
        }

        var result = new DisconnectResult(player);

        if (match.Phase == VsMatchPhase.GameCompleted)
        {
            player.IsConnected = false;
            result.ReleasePlayer = true;
            result.RemoveMatch = match.Players.All(item => !item.IsConnected);
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

        ContinueAfterDisconnectLocked(match, player, result);

        if (!result.ReleasePlayer &&
            match.Players.Any(item => item.IsConnected))
        {
            result.Messages =
                VsMatchSnapshotBuilder.BuildMessages(match);
        }

        return result;
    }

    private void ContinueAfterDisconnectLocked(
        VsMatchSession match,
        VsMatchPlayerState player,
        DisconnectResult result)
    {
        if (result.ReleasePlayer)
            return;

        if (match.Players.All(item => item.IsBot))
        {
            match.IsClosed = true;
            match.PhaseTimerCts.Cancel();
            result.AbandonedReward =
                VsMatchRewardCalculator.Calculate(match);
            match.Reward = result.AbandonedReward;
            result.RemoveMatch = true;

            AddLog(
                match,
                null,
                "AllPlayersDisconnected",
                string.Empty);
            return;
        }

        if (IsPreparationPhase(match.Phase))
        {
            AdvanceIfReadyLocked(match);
        }
        else if (IsAnswerPhase(match.Phase) &&
                 VsMatchGameRules.HaveAllParticipantsAnswered(match))
        {
            StartAnswerResultDelayLocked(match);
        }
        else if (IsAnswerPhase(match.Phase))
        {
            ScheduleBotAnswerLocked(match, player);
        }
    }

    /// <inheritdoc />
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

    private sealed class DisconnectResult
    {
        internal DisconnectResult(VsMatchPlayerState player)
        {
            Player = player;
        }

        internal VsMatchPlayerState Player { get; }
        internal bool ReleasePlayer { get; set; }
        internal bool RemoveMatch { get; set; }
        internal VsMatchRewardState? AbandonedReward { get; set; }
        internal (string ConnectionId, VsMatchSnapshot Snapshot)[] Messages
        {
            get;
            set;
        } = [];
    }
}
