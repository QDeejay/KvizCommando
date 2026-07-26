namespace KvizCommando.Shared.Contracts.VsGame;

public sealed class SaveBattleTeamRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int[] SelectedSlotNumbers { get; set; } = [];
}
