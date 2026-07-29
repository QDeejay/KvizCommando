using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Server.Services.VsGame.Matchmaking;

public interface IVsRankedQueueService
{
    IReadOnlyDictionary<int, int> GetConnectedPlayerCounts();

    Task<VsQueueJoinResult> JoinAsync(
        int playerId,
        string sessionId,
        string connectionId,
        int classificationId,
        CancellationToken ct = default);

    Task LeaveAsync(
        string connectionId,
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
}

/**
 * MÓDOSÍTÁS: a queue-belépés közvetlen, típusos eredményt ad a Hubnak.
 * A képernyő-DTO számára besorolásonkénti pillanatképet ad az összes
 * várakozó és már meccsben lévő, kapcsolódott játékosról.
 *
 * A rangsorolt várólista műveleteit és egy várakozó játékos
 * minimális, még nem meccssnapshotolt adatait írja le.
 */
