namespace KvizCommando.Client.Features.VsGame.ViewModels;

public sealed class VsBattleTeamVm
{
    public string Message { get; set; } = string.Empty;
    public VsBattleMemberVm[] Members { get; set; } = [];
    public VsClassificationLampVm[] ClassificationLamps { get; set; } = [];
    public bool CanSave { get; set; }
}

public sealed class VsBattleMemberVm
{
    public int SlotNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PictureCode { get; set; } = string.Empty;
    public string RankName { get; set; } = string.Empty;
    public string RankClassName { get; set; } = string.Empty;
    public string ClassificationText { get; set; } = string.Empty;
    public string OrientationShort { get; set; } = string.Empty;
    public int VitalityPercent { get; set; }
    public string VitalityCssClass { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    public bool IsSelectable { get; set; }
}

public sealed class VsClassificationLampVm
{
    public int ClassificationId { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string MinimumTeamLevelText { get; set; } = string.Empty;
    public string PartySizeText { get; set; } = string.Empty;
    public string RankClassZoneText { get; set; } = string.Empty;
    public string RequiredMembersText { get; set; } = string.Empty;
}

/**
 * MÓDOSÍTÁS: a ranking csapatkártya megjelenítési modellje a rövid
 * orientáció mellett a szám nélküli vitalitássáv adatait is tartalmazza.
 */
