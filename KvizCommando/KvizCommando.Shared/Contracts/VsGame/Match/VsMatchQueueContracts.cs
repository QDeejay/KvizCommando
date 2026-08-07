using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Shared.Contracts.VsGame.Match;

public sealed class VsConnectionCheckResult
{
    public int ResponseTimeMilliseconds { get; set; }
    public VsConnectionQuality Quality { get; set; }
}

public sealed class VsQueueJoinResult
{
    public bool IsAccepted { get; set; }
    public string ErrorKey { get; set; } = string.Empty;
}

public sealed class VsRankedQueueSnapshot
{
    public int ClassificationId { get; set; }
    public int WaitingPlayers { get; set; }
    public int RequiredPlayers { get; set; }
    public int RequiredPartySize { get; set; }
    public int Stake { get; set; }
    public DateTime? MatchmakingDeadlineUtc { get; set; }
    public VsMatchPlayerDto[] Players { get; set; } = [];
}

/**
 * MÓDOSÍTÁS: a queue-belépés közvetlen, típusos eredményt ad vissza,
 * ezért a várható validációs hibákhoz nem kell külön SignalR
 * CommandRejected esemény.
 *
 * A fájl a rangsorolt queue belépési eredményét és a kliensnek
 * küldhető, kizárólag publikus várólista-snapshotot tartalmazza.
 * MÓDOSÍTÁS: az egyszeri, szerveroldali kapcsolatellenőrzés eredménye
 * ugyanebben a queue-szerződésben utazik vissza a klienshez.
 * MÓDOSÍTÁS: a dinamikus matchmaking abszolút szerverhatárideje a
 * queue-snapshot része; a kliens ebből csak a kijelzést számolja.
 */
