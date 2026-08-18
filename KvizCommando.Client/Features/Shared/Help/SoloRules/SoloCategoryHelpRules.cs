using KvizCommando.Client.Data;
using KvizCommando.Shared.Constants;
using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Rules;

namespace KvizCommando.Client.Features.Shared.Help.SoloRules;

public static class SoloCategoryHelpRules
{
    public static IReadOnlyDictionary<string, string> Tokens { get; } =
        BuildTokens();

    private static IReadOnlyDictionary<string, string> BuildTokens()
    {
        var tokens = new Dictionary<string, string>
        {
            ["SOLO_ANSWER_SECONDS"] =
                SoloGameRules.ANSWER_SECONDS.ToString(),
            ["SOLO_FULL_POINTS_SECONDS"] =
                SoloGameRules.FULL_POINTS_SECONDS.ToString()
        };

        var scoreLimits = ScoreConstants.ScorLimits
            .Where(limit =>
                limit <= SoloGameRules.GetMaximumScore(
                    SoloGameMode.Category))
            .ToArray();

        for (var index = 0; index < scoreLimits.Length; index++)
        {
            tokens[$"SOLO_DEV_SCORE_{index + 1:00}"] =
                scoreLimits[index].ToString();
        }

        foreach (var reward in RankRewards.List)
        {
            var level = reward.RowIndex;
            var number = level.ToString("00");

            tokens[$"SOLO_LEVEL_{number}"] =
                RankNameTable.Data[level].PublicLevel!;
            tokens[$"SOLO_QUESTION_COUNT_{number}"] =
                SoloGameRules.GetQuestionCount(level).ToString();
            tokens[$"SOLO_MAX_CORRECT_POINTS_{number}"] =
                SoloGameRules.GetMaxPointsPerQuestion(level).ToString();
        }

        return tokens;
    }
}
