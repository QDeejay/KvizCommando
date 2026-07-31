using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed partial class VsMatchService
{
    private void CompleteMatchLocked(VsMatchSession match)
    {
        match.Reward = VsMatchRewardCalculator.Calculate(match);

        AddLog(
            match,
            null,
            "RewardCalculated",
            $"Players={match.Reward.Players.Length};" +
            $"PrizePool={match.Reward.PrizePool}");

        SaveRewardsPlaceholderLocked(match);
        StartPhaseLocked(match, VsMatchPhase.GameCompleted);
    }

    private static void SaveRewardsPlaceholderLocked(VsMatchSession match)
    {
        // TODO VS REWARD CACHE:
        // A match.Reward már minden PlayerCache-módosításhoz szükséges
        // adatot tartalmaz. Itt kell majd egyetlen cache-tranzakcióban
        // jóváírni a pozitív rewardot, levonni a segítségeket és az
        // energiát, illetve menteni a highscore-t.
        // A reward.Statistics tartalmazza a pont-/idő-, kategória-,
        // kérdezői és sajátkérdés-statisztikák növekményeit is.
        AddLog(
            match,
            null,
            "RewardSavePlaceholder",
            "PlayerCache persistence is not implemented yet.");
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

/**
 * ÚJ FÁJL: a kapitánykör commitja után egyszer elkészíti a végleges
 * reward-állapotot, kijelöli a későbbi cache-mentés pontos helyét,
 * majd a reward snapshot kiküldése után feloldja a botjátékosok
 * meccszárolását. A kapcsolódó emberek a saját kilépésükig maradnak.
 */
