using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums;

namespace KvizCommando.Server.Services.Rankings;

/// <summary>A szerver közös ranglistáinak frissítése és játékosonkénti lekérése.</summary>
internal interface IRankingCacheService
{
    /// <summary>A játékos friss eredményével összeállítja a négy megjelenítendő ranglistát.</summary>
    /// <param name="playerId">A bejelentkezett játékos azonosítója.</param>
    /// <param name="currentValues">A játékos négy aktuális pontszáma és ideje.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task<RankingDtos> GetForPlayerAsync(
        int playerId,
        IReadOnlyDictionary<RankingList, RankingValue> currentValues,
        CancellationToken ct = default);

    /// <summary>A mentési ciklus után új pillanatképet készít.</summary>
    Task RefreshAsync(CancellationToken ct = default);
}
