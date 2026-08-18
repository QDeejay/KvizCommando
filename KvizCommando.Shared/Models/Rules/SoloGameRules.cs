using KvizCommando.Shared.Constants;
using KvizCommando.Shared.Contracts.SoloGame;

namespace KvizCommando.Shared.Models.Rules;

public static class SoloGameRules
{
    public const int ANSWER_SECONDS = 25;
    public const int FULL_POINTS_SECONDS = 5;
    public const int POINT_DECREASE_REMAINING_SECONDS =
        ANSWER_SECONDS - FULL_POINTS_SECONDS;
    public const int WARNING_REMAINING_SECONDS = 10;
    public const int CRITICAL_REMAINING_SECONDS = 5;
    public const int HEART_REWARD_CORRECT_PERCENT = 50;
    public const int HEART_REWARD_DEVELOPMENT_POINTS = 1;
    public const int MEMBER_XP_SCORE_DIVISOR = 10;
    public const int MEMBER_XP_WRONG_ANSWER_DIVISOR = 5;

    /// <summary>
    /// Visszaadja az egyéni játék kérdéseinek számát.
    /// </summary>
    public static int GetQuestionCount(int level) =>
        level switch
        {
            <= 0 => 8,
            >= 19 => 20,
            _ => 10 + (level - 1) / 4 * 2
        };

    /// <summary>
    /// Visszaadja az egy kérdéssel megszerezhető legnagyobb pontszámot.
    /// </summary>
    public static int GetMaxPointsPerQuestion(int level) =>
        100 + level / 2 * 10;

    /// <summary>
    /// Visszaadja az adott játékszabályok mellett elérhető legnagyobb pontszámot.
    /// </summary>
    public static int GetMaximumScore(int level) =>
        GetQuestionCount(level) *
        GetMaxPointsPerQuestion(level);

    /// <summary>
    /// Visszaadja az adott játékszabályok mellett elérhető legnagyobb pontszámot.
    /// </summary>
    public static int GetMaximumScore(SoloGameMode mode) =>
        mode switch
        {
            SoloGameMode.Orientation =>
                GetMaximumScore(TeamRules.LAST_MEMBER_LEVEL),
            SoloGameMode.Category =>
                GetMaximumScore(RankRewards.List[^1].RowIndex),
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };

    /// <summary>
    /// Visszaadja a pontszámhoz tartozó fejlesztésipont-sávot.
    /// </summary>
    public static int GetScoreDevelopmentPointCount(int score) =>
        ScoreConstants.ScorLimits.Count(limit => score >= limit);

    /// <summary>
    /// Kiszámítja a pontszám alapján megszerzett fejlesztési pontokat.
    /// </summary>
    public static int GetEarnedScoreDevelopmentPoints(
        int newScore,
        int oldScore) =>
        Math.Max(
            GetScoreDevelopmentPointCount(newScore) -
            GetScoreDevelopmentPointCount(oldScore),
            0);

    /// <summary>
    /// Jelzi, hogy a játékos elérte-e a pontszámból szerezhető fejlesztési pontok maximumát.
    /// </summary>
    public static bool HasMaxedScoreDevelopmentPoints(
        int bestScore,
        int level) =>
        !ScoreConstants.ScorLimits.Any(limit =>
            limit > bestScore &&
            limit <= GetMaximumScore(level));

    /// <summary>
    /// Visszaadja a szívjutalomhoz szükséges helyes válaszok számát.
    /// </summary>
    public static int GetHeartRewardRequiredCorrectAnswers(
        int questionCount) =>
        (int)Math.Ceiling(
            questionCount *
            HEART_REWARD_CORRECT_PERCENT /
            100d);

    /// <summary>
    /// Jelzi, hogy a játékos megszerezte-e a szívjutalmat.
    /// </summary>
    public static bool HasEarnedHeartReward(
        int correctAnswers,
        int questionCount) =>
        correctAnswers >=
        GetHeartRewardRequiredCorrectAnswers(questionCount);

    /// <summary>
    /// Kiszámítja a válaszért járó pontszámot.
    /// </summary>
    public static int GetAnswerPoints(
        int maximumPoints,
        int elapsedMs)
    {
        var decreasingTimeMs = Math.Clamp(
            elapsedMs - FULL_POINTS_SECONDS * 1000,
            0,
            (ANSWER_SECONDS - FULL_POINTS_SECONDS) * 1000);
        var multiplier = 1.0 -
            decreasingTimeMs /
            ((ANSWER_SECONDS - FULL_POINTS_SECONDS) * 1000d);

        return (int)Math.Round(
            maximumPoints * multiplier,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Kiszámítja a válaszért járó pontszámot.
    /// </summary>
    public static int GetAnswerPoints(
        int maximumPoints,
        int elapsedMs,
        bool? isCorrect)
    {
        if (isCorrect is null)
            return 0;

        var points = GetAnswerPoints(maximumPoints, elapsedMs);
        return isCorrect.Value ? points : -points;
    }

    // A nyugdíjazási szint rangosztályt vált, de nem ad karaktertapasztalatot.
    /// <summary>
    /// Jelzi, hogy az adott szinten szerezhető-e karaktertapasztalat.
    /// </summary>
    public static bool CanEarnMemberExperience(int level) =>
        level + 1 != TeamRules.RETIRE_REWARD_RANK &&
        TeamRules.IsRankClassChangingPromotion(level);

    /// <summary>
    /// Kiszámítja az egyéni játék után járó karaktertapasztalatot.
    /// </summary>
    public static int GetMemberExperience(
        int basePoints,
        int correctAnswers,
        int wrongAnswers,
        int level)
    {
        if (!CanEarnMemberExperience(level))
            return 0;

        var score =
            basePoints * correctAnswers -
            basePoints * wrongAnswers /
            MEMBER_XP_WRONG_ANSWER_DIVISOR;

        return Math.Max(
            score / MEMBER_XP_SCORE_DIVISOR,
            0);
    }

    /// <summary>
    /// Kiszámítja az egyéni játék után járó csapattapasztalatot.
    /// </summary>
    public static int GetTeamExperience(
        int memberExperience,
        int level) =>
        memberExperience /
        RankRewards.List[level].MaxCharacters;
}
