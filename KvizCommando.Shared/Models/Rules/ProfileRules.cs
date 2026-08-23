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

    /// <summary>Visszaadja a csapatnév módosításához szükséges rangot.</summary>
    public static int GetTeamNameRequiredRank() =>
        GetRequiredRank(TEAM_NAME_REWARD_NO);

    /// <summary>Visszaadja az avatar módosításához szükséges rangot.</summary>
    public static int GetAvatarRequiredRank() =>
        GetRequiredRank(AVATAR_REWARD_NO);

    /// <summary>Megkeresi a jutalomhoz tartozó szükséges rangot.</summary>
    /// <param name="rewardNo">A rangjutalom súgóazonosítója.</param>
    /// <returns>A szükséges rang, vagy <c>0</c>, ha nincs ilyen jutalom.</returns>
    public static int GetRequiredRank(int rewardNo) =>
        RankRewards.List
            .FirstOrDefault(reward => reward.HelpRewardNo == rewardNo)
            ?.RowIndex ?? 0;

    /// <summary>Kiszámítja a következő engedélyezett csapatnévváltás időpontját.</summary>
    /// <param name="teamNameChangedUtc">Az előző módosítás UTC-időpontja.</param>
    /// <returns>A következő módosítás UTC-időpontja, vagy <see langword="null"/>, ha még nem történt módosítás.</returns>
    public static DateTime? GetNextTeamNameChangeUtc(
        DateTime? teamNameChangedUtc) =>
        teamNameChangedUtc?.AddDays(TEAM_NAME_CHANGE_COOLDOWN_DAYS);

    /// <summary>Ellenőrzi, hogy a játékos rangja és várakozási ideje engedi-e a csapatnévváltást.</summary>
    /// <param name="rankEnum">A játékos rangja.</param>
    /// <param name="teamNameChangedUtc">Az előző módosítás UTC-időpontja.</param>
    /// <param name="utcNow">Az ellenőrzés UTC-időpontja.</param>
    public static bool CanChangeTeamName(
        int rankEnum,
        DateTime? teamNameChangedUtc,
        DateTime utcNow) =>
        rankEnum >= GetTeamNameRequiredRank() &&
        (!teamNameChangedUtc.HasValue ||
         GetNextTeamNameChangeUtc(teamNameChangedUtc) <= utcNow);

    /// <summary>Ellenőrzi, hogy a játékos rangja engedi-e az avatar módosítását.</summary>
    /// <param name="rankEnum">A játékos rangja.</param>
    public static bool CanChangeAvatar(int rankEnum) =>
        rankEnum >= GetAvatarRequiredRank();

    /// <summary>Érvényes avatarszámot olvas ki a tárolt értékből.</summary>
    /// <param name="value">A tárolt avatarszám.</param>
    /// <returns>Az érvényes avatarszám, hibás értéknél az alapértelmezett avatar száma.</returns>
    public static int GetAvatarNumber(string? value) =>
        int.TryParse(value, out var avatarNumber) &&
        avatarNumber is >= MIN_AVATAR_NO and <= MAX_AVATAR_NO
            ? avatarNumber
            : DEFAULT_AVATAR_NO;
}
