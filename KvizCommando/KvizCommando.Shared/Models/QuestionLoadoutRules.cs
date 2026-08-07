namespace KvizCommando.Shared.Models;

public static class QuestionLoadoutRules
{
    public const int MAX_LOADOUT_SIZE = 10;
    public const int OWN_QUESTION_CATEGORY = 17;

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

    public static int GetOwnQuestionLimit(
        int loadoutSize,
        int occupiedUserSlots)
    {
        return Math.Min(
            Math.Max(loadoutSize, 0) / 2,
            Math.Max(occupiedUserSlots, 0));
    }
}

/**
 * ÚJ FÁJL: a kérdés-loadout szintfüggő méretét és a saját kérdések
 * közös felső korlátját egy helyen számolja a kliens, a kérdéskezelő
 * és a VS meccsmotor számára.
 */
