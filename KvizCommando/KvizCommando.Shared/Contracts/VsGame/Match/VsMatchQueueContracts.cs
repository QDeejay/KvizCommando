namespace KvizCommando.Shared.Contracts.VsGame.Match;

public sealed class VsRankedQueueSnapshot
{
    public int ClassificationId { get; set; }
    public int WaitingPlayers { get; set; }
    public int RequiredPlayers { get; set; }
    public int RequiredPartySize { get; set; }
    public int Stake { get; set; }
    public VsMatchPlayerDto[] Players { get; set; } = [];
}

/**
 * MÓDOSÍTÁS: a lobby látványtervéhez a kötelező csapatméretet és a
 * várakozók publikus rosteradatait is továbbítja.
 *
 * A rangsorolt várólista kliensnek küldhető, kizárólag publikus
 * állapotát tartalmazza.
 */
