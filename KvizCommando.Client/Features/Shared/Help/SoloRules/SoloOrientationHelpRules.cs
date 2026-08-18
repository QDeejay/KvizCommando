using KvizCommando.Client.Data;
using KvizCommando.Shared.Constants;
using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Models.Rules;

namespace KvizCommando.Client.Features.Shared.Help.SoloRules;

public static class SoloOrientationHelpRules
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
                SoloGameRules.FULL_POINTS_SECONDS.ToString(),
            ["SOLO_HEART_PERCENT"] =
                SoloGameRules.HEART_REWARD_CORRECT_PERCENT.ToString(),
            ["SOLO_HEART_DEV_POINTS"] =
                SoloGameRules.HEART_REWARD_DEVELOPMENT_POINTS.ToString(),
            ["SOLO_XP_LEVELS"] = string.Join(
                ", ",
                TeamRules.MemberLevels
                    .Where(SoloGameRules.CanEarnMemberExperience)
                    .Select(level =>
                        RankNameTable.Data[level].PublicLevel!))
        };

        var scoreLimits = ScoreConstants.ScorLimits
            .Where(limit =>
                limit <= SoloGameRules.GetMaximumScore(
                    SoloGameMode.Orientation))
            .ToArray();

        for (var index = 0; index < scoreLimits.Length; index++)
        {
            tokens[$"SOLO_DEV_SCORE_{index + 1:00}"] =
                scoreLimits[index].ToString();
        }

        foreach (var level in TeamRules.MemberLevels)
        {
            var number = level.ToString("00");
            var questionCount = SoloGameRules.GetQuestionCount(level);

            tokens[$"SOLO_LEVEL_{number}"] =
                RankNameTable.Data[level].PublicLevel!;
            tokens[$"SOLO_QUESTION_COUNT_{number}"] =
                questionCount.ToString();
            tokens[$"SOLO_MAX_CORRECT_POINTS_{number}"] =
                SoloGameRules.GetMaxPointsPerQuestion(level).ToString();
            tokens[$"SOLO_HEART_CORRECT_{number}"] =
                SoloGameRules
                    .GetHeartRewardRequiredCorrectAnswers(questionCount)
                    .ToString();
        }

        return tokens;
    }
}
