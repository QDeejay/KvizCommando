namespace KvizCommando.Server.Services.VsGame.Match;

public sealed class VsMatchRewardState
{
    public int PrizePool { get; init; }
    public VsMatchPlayerRewardState[] Players { get; init; } = [];
}

public sealed class VsMatchPlayerRewardState
{
    public int PlayerId { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public int OriginalPosition { get; init; }
    public int FinalPosition { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string TeamName { get; init; } = string.Empty;
    public int TeamLevel { get; init; }
    public bool IsBot { get; init; }
    public bool IsWinner { get; init; }
    public int ActualPoints { get; init; }
    public double ActualTimeSeconds { get; init; }
    public int FinalPoints { get; init; }
    public double FinalTimeSeconds { get; init; }
    public int CharacterAverageXp { get; init; }
    public int ScoreXp { get; init; }
    public int TeamXp { get; init; }
    public int StakeReturn { get; init; }
    public int BaseCreditReward { get; init; }
    public int TeamBonusCredit { get; init; }
    public int TeamBonusPercent { get; init; }
    public int CreditReward { get; init; }
    public int[] ConsumedHelps { get; init; } = new int[4];
    public VsMatchCharacterRewardState[] Characters { get; init; } = [];
    public VsMatchStatisticsState Statistics { get; init; } = new();
}

public sealed class VsMatchCharacterRewardState
{
    public int SlotNumber { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PictureCode { get; init; } = string.Empty;
    public int CharacterXp { get; init; }
    public int EnergyLoss { get; init; }
    public int Pension { get; init; }
    public int PlayDuels { get; init; }
    public int WinDuels { get; init; }
}

/**
 * ÚJ FÁJL: a lezárt meccs teljes jutalomeredménye. A későbbi
 * PlayerCache-íráshoz szükséges játékos-, session-, segítség-,
 * karakter-, részletezett csapat-XP-, energia-, nyugdíj- és
 * kreditadatokat akkor is
 * megőrzi, amikor egy bot zárolása már feloldható.
 * A később egy lépésben menthető meccsstatisztika és team bonus
 * százalék szintén itt áll rendelkezésre.
 * MÓDOSÍTÁS: karakterenként a PlayDuels/WinDuels növekményt
 * is továbbítja a PlayerCache-mentésnek.
 */
