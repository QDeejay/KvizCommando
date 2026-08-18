namespace KvizCommando.Shared.Models.Rules;

/// <summary>
/// A rangsorolt mérkőzés szerver- és kliensoldalon közösen használt játékszabályai.
/// </summary>
public static class VsRankedMatchRules
{
    public const int PREPARATION_SECONDS = 20;
    public const int GUESS_SECONDS = 20;
    public const int QUESTION_SECONDS = 20;
    public const int POINT_UNIT = 1;
    public const int CAPTAIN_MULTIPLIER = 2;
    public const int TIME_FREEZE_WRONG_ANSWER_PENALTY_SECONDS = 20;
    public const double WINNER_COMPENSATION_MIN = 1.0;
    public const double WINNER_COMPENSATION_MAX = 3.0;

    /// <summary>
    /// Kiszámítja a győztesnek járó szintkompenzációt.
    /// </summary>
    /// <param name="teamLevel">A csapat aktuális szintje.</param>
    public static double GetWinnerCompensation(int teamLevel)
    {
        var maximumLevel = RankRewards.List.Count - 1;
        var level = Math.Clamp(teamLevel, 1, maximumLevel);

        return Math.Round(
            1.0 + 2.0 * (level - 1) / (maximumLevel - 1),
            1,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Kiszámítja a rangsorolt mérkőzés eredménypontját.
    /// </summary>
    /// <param name="netPoints">A kompenzáció előtti nettó rangsorpont.</param>
    /// <param name="winnerCompensation">A győztes szintkülönbség miatti kompenzációja.</param>
    public static double GetRankedScore(
        int netPoints,
        double winnerCompensation) =>
        Math.Round(
            netPoints * winnerCompensation,
            1,
            MidpointRounding.AwayFromZero);
}
