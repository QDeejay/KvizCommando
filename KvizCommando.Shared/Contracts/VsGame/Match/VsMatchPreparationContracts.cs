using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Shared.Contracts.VsGame.Match;

public sealed class VsLoadoutAssignmentRequest
{
    public int LoadoutPosition { get; set; }
    public int RoundNumber { get; set; }
}

public sealed class VsHelpAssignmentRequest
{
    public VsHelpType HelpType { get; set; }
    public int RoundNumber { get; set; }
}
