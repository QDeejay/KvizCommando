using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Matchmaking;

public interface IVsRankedQueueService
{
    /// <summary>
    /// Visszaadja a rangsorolt várólistákhoz csatlakozott játékosok számát.
    /// </summary>
    IReadOnlyDictionary<int, int> GetConnectedPlayerCounts();

    /// <summary>
    /// Belépteti a játékost a kiválasztott rangsorolt várólistába.
    /// </summary>
    Task<VsQueueJoinResult> JoinAsync(
        int playerId,
        string sessionId,
        string connectionId,
        int classificationId,
        int responseTimeMilliseconds,
        VsConnectionQuality connectionQuality,
        CancellationToken ct = default);

    /// <summary>
    /// Eltávolítja a játékost a rangsorolt várólistából.
    /// </summary>
    Task<VsQueueLeaveStatus> LeaveAsync(
        string connectionId,
        CancellationToken ct = default);

    /// <summary>
    /// Feldolgozza a klienskapcsolat megszakadását.
    /// </summary>
    Task DisconnectAsync(
        string connectionId,
        CancellationToken ct = default);

    /// <summary>
    /// Eltávolítja a játékost a várólistából vagy a hozzá tartozó meccsből.
    /// </summary>
    Task LeavePlayerAsync(
        int playerId,
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
