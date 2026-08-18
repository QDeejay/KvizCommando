using KvizCommando.Server.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace KvizCommando.Server.Services.UserPlayerIdCache
{
    /// <summary>
    /// Az ASP.NET Identity felhasználóazonosítóját gyorsan játékosazonosítóhoz rendeli.
    /// Az adatokat 30 perc inaktivitás után automatikusan eldobja az IMemoryCache.
    /// </summary>
    public sealed class UserPlayerIdCacheService : IUserPlayerIdCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ApplicationDbContext _db;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public UserPlayerIdCacheService(IMemoryCache cache, ApplicationDbContext db)
        {
            _cache = cache;
            _db = db;

            _cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30),
                Priority = CacheItemPriority.Normal
            };
        }

        /// <summary>
        /// Visszaadja az ASP.NET Identity felhasználóazonosítójához tartozó játékosazonosítót.
        /// Hiányzó érték esetén adatbázisból tölt, majd 30 percre gyorsítótáraz.
        /// </summary>
        public async Task<int?> GetPlayerIdAsync(string userId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            if (_cache.TryGetValue(userId, out int pid))
            {
                Console.WriteLine("------------ Cahcel adott player id -------------------");
                return pid;
            }
               

            pid = await _db.Players
                .Where(p => p.UserId == userId)
                .Select(p => p.PlayerId)
                .FirstOrDefaultAsync(ct);

            if (pid == 0)
                return null;

            _cache.Set(userId, pid, _cacheOptions);
            Console.WriteLine("------------Db-vel adott player id -------------------");
            return pid;
        }

        /// <summary>
        /// Eltávolítja a megadott felhasználó játékosazonosítóját a gyorsítótárból.
        /// </summary>
        public void Invalidate(string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
                _cache.Remove(userId);
        }

        /// <summary>
        /// Kiüríti a teljes felhasználó–játékos leképezési gyorsítótárat.
        /// </summary>
        public void Clear()
        {
            if (_cache is MemoryCache memoryCache)
                memoryCache.Compact(1.0);
        }
    }
}
