namespace KvizCommando.Shared.Models.Rules;

/// <summary>
/// A csapatprofil név- és avatarbeállításainak közös szabályai.
/// </summary>
public static class ProfileRules
{
    public const int TEAM_NAME_REWARD_NO = 201;
    public const int AVATAR_REWARD_NO = 205;
    public const int TEAM_NAME_CHANGE_COOLDOWN_DAYS = 30;
    public const int MIN_AVATAR_NO = 0;
    public const int MAX_AVATAR_NO = 20;
    public const int DEFAULT_AVATAR_NO = 0;

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

    public static int GetAvatarNumber(string? value) =>
        int.TryParse(value, out var avatarNumber) &&
        avatarNumber is >= MIN_AVATAR_NO and <= MAX_AVATAR_NO
            ? avatarNumber
            : DEFAULT_AVATAR_NO;
}
