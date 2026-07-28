namespace KvizCommando.Shared.Contracts.VsGame.Match;

public sealed class VsRankedQueueSnapshot
{
    public int ClassificationId { get; set; }
    public int WaitingPlayers { get; set; }
    public int RequiredPlayers { get; set; }
    public int Stake { get; set; }
}

/**
 * A rangsorolt várólista kliensnek küldhető, kizárólag publikus
 * állapotát tartalmazza.
 */
