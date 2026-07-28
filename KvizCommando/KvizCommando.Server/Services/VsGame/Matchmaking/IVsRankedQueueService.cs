namespace KvizCommando.Server.Services.VsGame.Matchmaking;

public interface IVsRankedQueueService
{
    Task JoinAsync(
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
 * A rangsorolt várólista műveleteit és egy várakozó játékos
 * minimális, még nem meccssnapshotolt adatait írja le.
 */
