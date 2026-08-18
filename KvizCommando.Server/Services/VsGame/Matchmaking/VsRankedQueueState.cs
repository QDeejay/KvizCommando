namespace KvizCommando.Server.Services.VsGame.Matchmaking;

internal sealed class VsRankedQueueState
{
    public List<VsRankedQueueEntry> Entries { get; } = [];
    public DateTime? MatchmakingDeadlineUtc { get; set; }
    public bool ArrivalExtensionUsed { get; set; }
}
