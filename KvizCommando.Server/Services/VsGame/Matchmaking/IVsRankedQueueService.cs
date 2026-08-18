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

    Task<VsQueueLeaveStatus> LeaveAsync(
        string connectionId,
        CancellationToken ct = default);

    Task DisconnectAsync(
        string connectionId,
        CancellationToken ct = default);

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
