using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Matchmaking;

public interface IVsRankedQueueService
{
    IReadOnlyDictionary<int, int> GetConnectedPlayerCounts();

    Task<VsQueueJoinResult> JoinAsync(
        int playerId,
        string sessionId,
        string connectionId,
        int classificationId,
        int responseTimeMilliseconds,
        VsConnectionQuality connectionQuality,
        CancellationToken ct = default);

    Task<bool> LeaveAsync(
        string connectionId,
        CancellationToken ct = default);

    Task DisconnectAsync(
        string connectionId,
        CancellationToken ct = default);

    Task LeavePlayerAsync(
        int playerId,
        string sessionId,
        CancellationToken ct = default);
}

public sealed class VsRankedQueueEntry
{
    public int PlayerId { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public string ConnectionId { get; init; } = string.Empty;
    public int ClassificationId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string TeamName { get; init; } = string.Empty;
    public int TeamLevel { get; init; }
    public int ResponseTimeMilliseconds { get; init; }
    public VsConnectionQuality ConnectionQuality { get; init; }
}

/**
 * MÓDOSÍTÁS: a queue-belépés közvetlen, típusos eredményt ad a Hubnak.
 * A képernyő-DTO számára besorolásonkénti pillanatképet ad az összes
 * várakozó és már meccsben lévő, kapcsolódott játékosról.
 *
 * A rangsorolt várólista műveleteit és egy várakozó játékos
 * minimális, még nem meccssnapshotolt adatait írja le.
 * MÓDOSÍTÁS: a belépéskor már szerveroldalon megmért válaszidőt és
 * minősítést a queue-entry továbbviszi a későbbi roster snapshotokba.
 * MÓDOSÍTÁS: logoutkor PlayerId és SessionId alapján is eltávolítható
 * a várakozó játékos.
 * MÓDOSÍTÁS: a manuális queue-kilépés és a kapcsolatvesztés külön
 * művelet; csak a manuális kilépés hoz létre újrabelépési tiltást.
 */
