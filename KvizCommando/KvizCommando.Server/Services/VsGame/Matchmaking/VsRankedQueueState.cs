namespace KvizCommando.Server.Services.VsGame.Matchmaking;

internal sealed class VsRankedQueueState
{
    public List<VsRankedQueueEntry> Entries { get; } = [];
    public DateTime? MatchmakingDeadlineUtc { get; set; }
    public bool ArrivalExtensionUsed { get; set; }
}

/**
 * ÚJ FÁJL: egy harci besorolás várólistájának minimális szerveroldali
 * időzítési állapotát tartja. Játék- vagy kliensállapotot nem kezel.
 */
