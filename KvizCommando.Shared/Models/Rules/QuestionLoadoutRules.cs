namespace KvizCommando.Shared.Models.Rules;

public static class QuestionLoadoutRules
{
    public const int MAX_LOADOUT_SIZE = 10;
    public const int OWN_QUESTION_CATEGORY = 17;

    /// <summary>
    /// Visszaadja a csapatszinthez tartozó kérdéslista méretét.
    /// </summary>
    /// <param name="teamLevel">A csapat aktuális szintje.</param>
    public static int GetLoadoutSize(int teamLevel)
    {
        if (teamLevel <= 0)
            return 0;

        var level = Math.Clamp(
            teamLevel,
            1,
            RankRewards.List.Count - 1);

        return Math.Min(
            RankRewards.List[level].MaxCharacters * 2,
            MAX_LOADOUT_SIZE);
    }

    /// <summary>
    /// Visszaadja a kérdéslistában használható saját kérdések legnagyobb számát.
    /// </summary>
    /// <param name="loadoutSize">A kérdés-összeállítás teljes mérete.</param>
    /// <param name="occupiedUserSlots">A saját kérdéssel már elfoglalt helyek száma.</param>
    public static int GetOwnQuestionLimit(
        int loadoutSize,
        int occupiedUserSlots)
    {
        return Math.Min(
            Math.Max(loadoutSize, 0) / 2,
            Math.Max(occupiedUserSlots, 0));
    }
}
