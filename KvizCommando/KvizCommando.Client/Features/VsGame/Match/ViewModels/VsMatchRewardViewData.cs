namespace KvizCommando.Client.Features.VsGame.Match.ViewModels;

public sealed class VsMatchRewardViewData
{
    public int PrizePool { get; set; }
    public VsRewardStandingVm[] Standings { get; set; } = [];
    public VsMyRewardVm? MyReward { get; set; }
}

public sealed class VsRewardStandingVm
{
    public int FinalPosition { get; set; }
    public int PlayerPosition { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string TeamLevel { get; set; } = string.Empty;
    public bool IsMe { get; set; }
    public bool IsBot { get; set; }
    public bool IsWinner { get; set; }
    public int Points { get; set; }
    public double TimeSeconds { get; set; }
}

public sealed class VsMyRewardVm
{
    public int FinalPosition { get; set; }
    public bool IsTeamXpAvailable { get; set; }
    public int TeamXp { get; set; }
    public int StakeReturn { get; set; }
    public int BaseCreditReward { get; set; }
    public int TeamBonusCredit { get; set; }
    public int TeamBonusPercent { get; set; }
    public int CreditReward { get; set; }
    public VsConsumedHelpVm[] ConsumedHelps { get; set; } = [];
    public VsCharacterRewardVm[] Characters { get; set; } = [];
}

public sealed class VsConsumedHelpVm
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class VsCharacterRewardVm
{
    public int SlotNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PictureCode { get; set; } = string.Empty;
    public int CharacterXp { get; set; }
    public int EnergyLoss { get; set; }
    public int Pension { get; set; }
}

/**
 * ÚJ FÁJL: a meccs végi tabella és a saját csapat-/karakterjutalom
 * tisztán megjelenítési modelljei. Sem SignalR-, sem cachelogikát nem
 * tartalmaznak. A csapat-XP sor láthatósága és a team bonus százaléka
 * közvetlen szerveres rewardadatból érkezik.
 */
