using KvizCommando.Server.Domain.Entities.Players;
using System.Threading;
using System.Threading.Tasks;

namespace KvizCommando.Server.Services.Players
{
    public interface IPlayerService
    {
        /// <summary>
        /// Inaktiválja a játékost cache-ben, ha van, azonosított UserId alapján.
        /// </summary>
        Task LogoutAndRemoveCacheAsync(string userId, CancellationToken ct = default);

       /// Task<int> GetPlayerIdAsync(string userId, CancellationToken ct = default);
    }
}
