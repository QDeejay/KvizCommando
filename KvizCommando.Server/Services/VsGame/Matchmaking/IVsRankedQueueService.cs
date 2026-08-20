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
    /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
    /// <param name="sessionId">A várólistára lépéskor ellenőrzött munkamenet-azonosító.</param>
    /// <param name="connectionId">Az aktív SignalR-kapcsolat azonosítója.</param>
    /// <param name="classificationId">A kiválasztott rangsorolt várólista azonosítója.</param>
    /// <param name="responseTimeMilliseconds">A kliens és a szerver között mért válaszidő ezredmásodpercben.</param>
    /// <param name="connectionQuality">A mért válaszidőből meghatározott kapcsolatminőség.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
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
    /// <param name="connectionId">Az aktív SignalR-kapcsolat azonosítója.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task<VsQueueLeaveStatus> LeaveAsync(
        string connectionId,
        CancellationToken ct = default);

    /// <summary>
    /// Feldolgozza a klienskapcsolat megszakadását.
    /// </summary>
    /// <param name="connectionId">Az aktív SignalR-kapcsolat azonosítója.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task DisconnectAsync(
        string connectionId,
        CancellationToken ct = default);

    /// <summary>
    /// Eltávolítja a játékost a várólistából vagy a hozzá tartozó meccsből.
    /// </summary>
    /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
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
    public string CaptainAvatar { get; init; } = "0";
    public int TeamLevel { get; init; }
    public int ResponseTimeMilliseconds { get; init; }
    public VsConnectionQuality ConnectionQuality { get; init; }
}
