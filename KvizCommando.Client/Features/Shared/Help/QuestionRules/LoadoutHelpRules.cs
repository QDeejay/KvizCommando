using KvizCommando.Client.Data;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Rules;

namespace KvizCommando.Client.Features.Shared.Help.QuestionRules;

public static class LoadoutHelpRules
{
    private const int LOADOUT_SIZE_01 = 6;
    private const int LOADOUT_SIZE_02 = 8;
    private const int LOADOUT_SIZE_03 =
        QuestionLoadoutRules.MAX_LOADOUT_SIZE;

    public static IReadOnlyDictionary<string, string> Tokens { get; } =
        BuildTokens();

    private static IReadOnlyDictionary<string, string> BuildTokens()
    {
        var startRank01 = FindStartRank(LOADOUT_SIZE_01);
        var startRank02 = FindStartRank(LOADOUT_SIZE_02);
        var startRank03 = FindStartRank(LOADOUT_SIZE_03);

        return new Dictionary<string, string>
        {
            ["LOADOUT_SIZE_01"] = LOADOUT_SIZE_01.ToString(),
            ["LOADOUT_SIZE_02"] = LOADOUT_SIZE_02.ToString(),
            ["LOADOUT_SIZE_03"] = LOADOUT_SIZE_03.ToString(),
            ["LOADOUT_PUBLIC_LEVEL_START_01"] =
                GetPublicLevel(startRank01),
            ["LOADOUT_PUBLIC_LEVEL_END_01"] =
                GetPublicLevel(startRank02 - 1),
            ["LOADOUT_PUBLIC_LEVEL_START_02"] =
                GetPublicLevel(startRank02),
            ["LOADOUT_PUBLIC_LEVEL_END_02"] =
                GetPublicLevel(startRank03 - 1),
            ["LOADOUT_PUBLIC_LEVEL_START_03"] =
                GetPublicLevel(startRank03)
        };
    }

    private static int FindStartRank(int loadoutSize) =>
        RankRewards.List
            .First(row =>
                QuestionLoadoutRules.GetLoadoutSize(row.RowIndex) ==
                loadoutSize)
            .RowIndex;

    private static string GetPublicLevel(int rankIndex) =>
        RankNameTable.Data[rankIndex].PublicLevel!;
}
