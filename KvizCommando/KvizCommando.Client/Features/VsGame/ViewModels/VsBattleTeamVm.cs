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
    public bool IsSelected { get; set; }
}

public sealed class VsClassificationLampVm
{
    public int ClassificationId { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
