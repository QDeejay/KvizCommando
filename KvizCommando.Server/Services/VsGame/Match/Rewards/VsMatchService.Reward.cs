using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed partial class VsMatchService
{
    private VsMatchRewardState CompleteMatchLocked(VsMatchSession match)
    {
        var reward = VsMatchRewardCalculator.Calculate(match);
        match.Reward = reward;

        AddLog(
            match,
            null,
            "RewardCalculated",
            $"Players={match.Reward.Players.Length};" +
            $"PrizePool={match.Reward.PrizePool}");

        StartPhaseLocked(match, VsMatchPhase.GameCompleted);
        return reward;
    }

    private void ReleaseCompletedBots(VsMatchSession match)
    {
        VsMatchPlayerState[] bots;
        var removeMatch = false;

        lock (match.SyncRoot)
        {
            if (match.IsClosed || match.Phase != VsMatchPhase.GameCompleted)
                return;

            bots = [.. match.Players.Where(player => player.IsBot)];
            removeMatch = match.Players.All(player => !player.IsConnected);
        }

        foreach (var bot in bots)
            _store.ReleasePlayer(match, bot);

        if (removeMatch)
            _store.TryRemove(match.MatchId, out _);
    }
}
