namespace KvizCommando.Shared.Models.Dtos;

public sealed class VsGameDtos
{
    public bool AccessDenied { get; set; }
    public VsRootBoxInfo RootBoxInfo { get; set; } = new();
    public VsRankedBattlefieldsDto RankedBattlefields { get; set; } = new();
}

public sealed class VsRootBoxInfo
{
    public bool IsCreateBattlefieldEnabled { get; set; }
    public bool IsJoinBattlefieldEnabled { get; set; }
    public bool IsRankedBattlefieldsEnabled { get; set; }

    public int BattleReadyCharacterCount { get; set; }
    public int RequiredBattleReadyCharacterCount { get; set; } = 3;
    public int CreditBalance { get; set; }
    public int RequiredCreditBalance { get; set; } = 50;
    public int TeamRank { get; set; }
    public int PrivatePlayerCount { get; set; }
    public int RankedPlayerCount { get; set; }
}

public sealed class VsRankedBattlefieldsDto
{
    public VsBattleMemberDto[] TeamMembers { get; set; } = [];
    public VsRankedSelectionDto SavedSelection { get; set; } = new();
    public VsBattleClassificationDto[] Classifications { get; set; } = [];
}

public sealed class VsBattleMemberDto
{
    public int SlotNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PictureCode { get; set; } = string.Empty;
    public int Rank { get; set; }
    public int RankClass { get; set; }
    public int OrientationId { get; set; }
    public int EnergyPoints { get; set; }
    public bool IsSelectable { get; set; }
}

public sealed class VsRankedSelectionDto
{
    public int[] SelectedSlotNumbers { get; set; } = [];
    public int[] EligibleClassificationIds { get; set; } = [];
}

public sealed class VsBattleClassificationDto
{
    public int ClassificationId { get; set; }
    public int Stake { get; set; }
    public int MinimumTeamRank { get; set; }
    public int RequiredPartySize { get; set; }
    public int MemberMinimumRankClass { get; set; }
    public int MemberMaximumRankClass { get; set; }
    public int RequiredMembersInRankClassRange { get; set; }
    public int PlayerCount { get; set; }
}

/**
 * MÓDOSÍTÁS: a harci besorolás DTO megkapta a szerver által
 * meghatározott ranked tétet, valamint a képernyő lekérésekor
 * rögzített privát, összes ranked és besorolásonkénti létszámot.
 * MÓDOSÍTÁS: a csapatösszeállító karakterkártyájához a tag fő
 * orientációazonosítóját is továbbítja.
 *
 * A fájl a VS menü, a mentett harci csapat és a besorolási
 * feltételrendszer képernyő-snapshotjait tartalmazza.
 */
