using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Rules;
using System.Text.Json;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed class VsMatchRewardPersistenceService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VsMatchRewardPersistenceService> _logger;

    public VsMatchRewardPersistenceService(
        IServiceScopeFactory scopeFactory,
        ILogger<VsMatchRewardPersistenceService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    internal async Task SaveAsync(
        Guid matchId,
        int playerCount,
        VsMatchRewardState reward)
    {
        foreach (var playerReward in reward.Players)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var cache = scope.ServiceProvider
                    .GetRequiredService<IPlayerCacheService>();

                var playerSaved =
                    await cache.UpdateRewardPlayerLockedAsync(
                    playerReward.PlayerId,
                    playerReward.SessionId,
                    player => ApplyPlayerReward(
                        player,
                        playerReward,
                        playerCount),
                    CancellationToken.None);

                if (playerSaved != true)
                {
                    LogSaveFailure(matchId, playerReward.PlayerId);
                    continue;
                }

                if (playerReward.IsBot ||
                    playerReward.Statistics.OwnQuestions.Count == 0)
                {
                    continue;
                }

                var questionsSaved =
                    await cache.UpdateRewardQuestionsLockedAsync(
                        playerReward.PlayerId,
                        playerReward.SessionId,
                        (_, questions) => ApplyOwnQuestionStatistics(
                            questions,
                            playerReward.Statistics),
                        CancellationToken.None);

                if (questionsSaved != true)
                    LogSaveFailure(matchId, playerReward.PlayerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "VS reward cache save failed. matchId={MatchId}, playerId={PlayerId}",
                    matchId,
                    playerReward.PlayerId);
            }
        }
    }

    private static DirtyFlags ApplyPlayerReward(
        CachedPlayer player,
        VsMatchPlayerRewardState reward,
        int playerCount)
    {
        var dirty = DirtyFlags.TeamStats;

        ApplyTeamStatistics(player, reward, playerCount);
        dirty |= ApplyHelpDeductions(player, reward.ConsumedHelps);
        dirty |= ApplyCharacterChanges(player, reward);

        if (reward.IsBot)
            return dirty;

        if (reward.TeamXp != 0 || reward.CreditReward != 0)
        {
            ApplyTeamXp(player, reward.TeamXp);
            player.Core.Credit = Math.Max(
                0,
                player.Core.Credit + reward.CreditReward);
            dirty |= DirtyFlags.Core;
        }

        if (reward.Statistics.QuestionsAsked != 0)
        {
            player.AskStats.TotalQuestionsAsked +=
                reward.Statistics.QuestionsAsked;
            player.AskStats.TotalAskPointsEarned +=
                reward.Statistics.CorrectAnswersToAskedQuestions;
            dirty |= DirtyFlags.AskStats;
        }

        if (reward.Statistics.Categories.Count != 0)
        {
            foreach (var matchStatistic in
                     reward.Statistics.Categories)
            {
                var category = player.CategoryStats.First(statistic =>
                    statistic.CategoryId == matchStatistic.Key);

                category.Answered += matchStatistic.Value.Answered;
                category.Correct += matchStatistic.Value.Correct;
            }

            dirty |= DirtyFlags.CategoryStats;
        }

        return dirty;
    }

    private static void ApplyTeamXp(
        CachedPlayer player,
        int teamXp)
    {
        player.Core.XP = Math.Max(0, player.Core.XP + teamXp);

        while (player.Core.RankEnum <= TeamRules.LAST_XP_LEVEL &&
               player.Core.XP >=
               RankRewards.List[player.Core.RankEnum].NextLevelTeam)
        {
            player.Core.RankEnum++;
            player.Core.DevPoint +=
                RankRewards.List[player.Core.RankEnum]
                    .DevPointToStore;
        }
    }

    private static void ApplyTeamStatistics(
        CachedPlayer player,
        VsMatchPlayerRewardState reward,
        int playerCount)
    {
        var statistics = player.TeamStats;
        statistics.RankedPlayed++;

        var placements = playerCount switch
        {
            2 => statistics.RankedPlacements.Players2,
            3 => statistics.RankedPlacements.Players3,
            4 => statistics.RankedPlacements.Players4,
            _ => null
        };

        if (placements is not null)
        {
            var position = reward.IsBot
                ? playerCount
                : reward.FinalPosition;
            placements[position - 1]++;
        }

        if (reward.IsBot)
            return;

        statistics.RankedGuessCount +=
            reward.Statistics.GuessCount;
        statistics.RankedGuessErrorTotal +=
            (decimal)reward.Statistics.GuessErrorTotal;

        if (reward.IsWinner)
            statistics.RankedWon++;

        if (reward.RankedScore <= 0 ||
            reward.RankedScore < statistics.RankedHighScore ||
            reward.RankedScore == statistics.RankedHighScore &&
            statistics.RankedHighScoreTime > 0 &&
            reward.ActualTimeSeconds >=
            statistics.RankedHighScoreTime)
        {
            return;
        }

        statistics.RankedHighScore = reward.RankedScore;
        statistics.RankedHighScoreTime = reward.ActualTimeSeconds;
    }

    private static DirtyFlags ApplyHelpDeductions(
        CachedPlayer player,
        IReadOnlyList<int> consumedHelps)
    {
        if (!consumedHelps.Any(count => count > 0))
            return DirtyFlags.None;

        var helpData = JsonSerializer.Deserialize<int[]>(
                           player.Loadout.HelpLevelsJson) ??
                       new int[8];

        for (var index = 0; index < consumedHelps.Count; index++)
        {
            helpData[index + 4] = Math.Max(
                0,
                helpData[index + 4] - consumedHelps[index]);
        }

        player.Loadout.HelpLevelsJson =
            JsonSerializer.Serialize(helpData);

        return DirtyFlags.Loadout;
    }

    private static DirtyFlags ApplyCharacterChanges(
        CachedPlayer player,
        VsMatchPlayerRewardState reward)
    {
        var changed = false;

        foreach (var characterReward in reward.Characters)
        {
            var character =
                player.Characters[characterReward.SlotNumber - 1];

            if (character is null)
                continue;

            if (characterReward.EnergyLoss > 0)
            {
                character.EnergyPoints = Math.Max(
                    0,
                    character.EnergyPoints -
                    characterReward.EnergyLoss);
                changed = true;
            }

            if (reward.IsBot)
                continue;

            character.XP += characterReward.CharacterXp;
            character.Pension += characterReward.Pension;
            character.CharStatistic.PlayDuels +=
                characterReward.PlayDuels;
            character.CharStatistic.WinDuels +=
                characterReward.WinDuels;

            changed |= characterReward.CharacterXp != 0 ||
                       characterReward.Pension != 0 ||
                       characterReward.PlayDuels != 0 ||
                       characterReward.WinDuels != 0;
        }

        return changed
            ? DirtyFlags.Characters
            : DirtyFlags.None;
    }

    private static uint ApplyOwnQuestionStatistics(
        CachedQuestion questions,
        VsMatchStatisticsState statistics)
    {
        var dirtyMask = 0u;

        for (var slot = 0; slot < questions.uSlots.Length; slot++)
        {
            var question = questions.uSlots[slot];

            if (question is null ||
                !statistics.OwnQuestions.TryGetValue(
                    question.Id,
                    out var increment))
            {
                continue;
            }

            question.Ask += increment.Asked;
            question.OkAnswer += increment.CorrectAnswers;
            dirtyMask |= 1u << slot;
        }

        return dirtyMask;
    }

    private void LogSaveFailure(Guid matchId, int playerId) =>
        _logger.LogError(
            "VS reward cache save was rejected. matchId={MatchId}, playerId={PlayerId}",
            matchId,
            playerId);
}
