namespace KvizCommando.Shared.Contracts.VsGame.Match;

public sealed class VsMatchRewardDto
{
    public int PrizePool { get; set; }
    public VsRewardStandingDto[] Standings { get; set; } = [];
    public VsMyRewardDto? MyReward { get; set; }
}

public sealed class VsRewardStandingDto
{
    public int FinalPosition { get; set; }
    public int PlayerPosition { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public int TeamLevel { get; set; }
    public bool IsMe { get; set; }
    public bool IsBot { get; set; }
    public bool IsWinner { get; set; }
    public int Points { get; set; }
    public double TimeSeconds { get; set; }
}

public sealed class VsMyRewardDto
{
    public int FinalPosition { get; set; }
    public bool IsBot { get; set; }
    public bool IsTeamXpAvailable { get; set; }
    public int CharacterAverageXp { get; set; }
    public int ScoreXp { get; set; }
    public int TeamXp { get; set; }
    public int NewTeamLevel { get; set; }
    public int StakeReturn { get; set; }
    public int BaseCreditReward { get; set; }
    public int TeamBonusCredit { get; set; }
    public int TeamBonusPercent { get; set; }
    public int CreditReward { get; set; }
    public int[] ConsumedHelps { get; set; } = new int[4];
    public VsCharacterRewardDto[] Characters { get; set; } = [];
}

public sealed class VsCharacterRewardDto
{
    public int SlotNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PictureCode { get; set; } = string.Empty;
    public int CharacterXp { get; set; }
    public bool IsCharacterXpCapped { get; set; }
    public int EnergyLoss { get; set; }
    public int Pension { get; set; }
}

/**
 * ÚJ FÁJL: a meccs végi nyilvános sorrend és a címzett saját
 * jutalmának kliensre küldhető contractjai. PlayerId, SessionId és
 * más szerveroldali mentési adat nem kerül a böngészőbe.
 * A kliens külön jogosultságot és levezetést kap a csapat-XP
 * megjelenítéséhez, valamint a team bonus százalékát is megkapja.
 * MÓDOSÍTÁS: karakterenként továbbítja, ha a megjelenített XP már a
 * következő szint határán levágott, tényleges jóváírás.
 * MÓDOSÍTÁS: tényleges szintlépéskor az új csapatszintet is továbbítja.
 */
