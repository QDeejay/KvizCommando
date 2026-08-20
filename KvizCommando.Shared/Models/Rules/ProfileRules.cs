using KvizCommando.Shared.Models;

namespace KvizCommando.Shared.Models.Rules;

/// <summary>
/// A csapatprofil név- és avatarbeállításainak közös szabályai.
/// </summary>
public static class ProfileRules
{
    public const int TEAM_NAME_REWARD_NO = 202;
    public const int AVATAR_REWARD_NO = 205;
    public const int TEAM_NAME_CHANGE_COOLDOWN_DAYS = 30;
    public const int AVATAR_COUNT = 21;
    public const int DEFAULT_AVATAR_NO = 1;

    public static int GetTeamNameRequiredRank() =>
        GetRequiredRank(TEAM_NAME_REWARD_NO);

    public static int GetAvatarRequiredRank() =>
        GetRequiredRank(AVATAR_REWARD_NO);

    public static int GetRequiredRank(int rewardNo) =>
        RankRewards.List
            .FirstOrDefault(reward => reward.HelpRewardNo == rewardNo)
            ?.RowIndex ?? 0;

    public static DateTime? GetNextTeamNameChangeUtc(
        DateTime? teamNameChangedUtc) =>
        teamNameChangedUtc?.AddDays(TEAM_NAME_CHANGE_COOLDOWN_DAYS);

    public static bool CanChangeTeamName(
        int rankEnum,
        DateTime? teamNameChangedUtc,
        DateTime utcNow) =>
        rankEnum >= GetTeamNameRequiredRank() &&
        (!teamNameChangedUtc.HasValue ||
         GetNextTeamNameChangeUtc(teamNameChangedUtc) <= utcNow);

    public static bool CanChangeAvatar(int rankEnum) =>
        rankEnum >= GetAvatarRequiredRank();

    public static bool TryGetAvatarNumber(
        string? value,
        out int avatarNumber) =>
        int.TryParse(value, out avatarNumber) &&
        avatarNumber is >= DEFAULT_AVATAR_NO and <= AVATAR_COUNT;
}
