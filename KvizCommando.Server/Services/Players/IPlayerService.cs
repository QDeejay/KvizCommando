using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Services.PlayerCache;
using System.Threading;
using System.Threading.Tasks;

namespace KvizCommando.Server.Services.Players
{
    public interface IPlayerService
    {
        /// <summary>
        /// Ellenőrzi, hogy a játékos munkamenete továbbra is érvényes-e.
        /// </summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CacheReadStatus> CheckSessionAsync(
            string userId,
            string sessionId,
            CancellationToken ct = default);

        /// <summary>
        /// Inaktiválja a játékost cache-ben, ha van, azonosított UserId alapján.
        /// </summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task LogoutAndRemoveCacheAsync(string userId, string sessionId, CancellationToken ct = default);
    }
}
