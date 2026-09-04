using KvizCommando.Client.Data;
using KvizCommando.Shared.Models;

namespace KvizCommando.Client.Features.Shared.Help.QuestionRules;

public static class UserPendingHelpRules
{
    public static IReadOnlyDictionary<string, string> Tokens { get; } =
        BuildTokens();

    private static IReadOnlyDictionary<string, string> BuildTokens()
    {
        var tokens = new Dictionary<string, string>();
        var userSlotSizes = RankRewards.List
            .Select(reward => reward.OwnQuestSlot)
            .Where(slotSize => slotSize > 0)
            .Distinct()
            .ToArray();
        var pendingSlotSizes = RankRewards.List
            .Select(GetPendingSlotSize)
            .Where(slotSize => slotSize > 0)
            .Distinct()
            .ToArray();

        AddSlotTokens(
            tokens,
            "USER_SLOT",
            userSlotSizes,
            reward => reward.OwnQuestSlot);

        AddSlotTokens(
            tokens,
            "PENDING_SLOT",
            pendingSlotSizes,
            GetPendingSlotSize);

        return tokens;
    }

    private static void AddSlotTokens(
        Dictionary<string, string> tokens,
        string prefix,
        int[] slotSizes,
        Func<RankRewardRow, int> getSlotSize)
    {
        var startRanks = slotSizes
            .Select(slotSize => RankRewards.List
                .First(reward => getSlotSize(reward) == slotSize)
                .RowIndex)
            .ToArray();

        for (var index = 0; index < slotSizes.Length; index++)
        {
            var number = index + 1;
            var slotSize = slotSizes[index];

            tokens[$"{prefix}_SIZE_{number:00}"] = slotSize.ToString();
            tokens[$"{prefix}_PUBLIC_LEVEL_START_{number:00}"] =
                GetPublicLevel(startRanks[index]);

            if (index < slotSizes.Length - 1)
            {
                tokens[$"{prefix}_PUBLIC_LEVEL_END_{number:00}"] =
                    GetPublicLevel(startRanks[index + 1] - 1);
            }
        }
    }

    private static string GetPublicLevel(int rankIndex) =>
        RankNameTable.Data[rankIndex].PublicLevel!;

    private static int GetPendingSlotSize(RankRewardRow reward) =>
        reward.OwnQuestSlot == 0
            ? 0
            : Math.Max(1, reward.OwnQuestSlot >> 1);
}
