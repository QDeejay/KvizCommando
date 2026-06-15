using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.UserPlayerIdCache;

namespace KvizCommando.Server.Services.Players
{
    public sealed class PlayerService : IPlayerService
    {
   
        private readonly IPlayerCacheService _cache;
        private readonly IUserPlayerIdCacheService _idCasche;
        private readonly ILogger<PlayerService> _logger;

        public PlayerService(IPlayerCacheService cache, ILogger<PlayerService> logger, IUserPlayerIdCacheService userPlayerId)
        {
          
            _cache = cache;
            _logger = logger;
            _idCasche = userPlayerId;
        }

        public async Task LogoutAndRemoveCacheAsync(string userId, CancellationToken ct = default)
        {
            var sessionId = "Teszt"; // nincs jelentősége, csak egyedi legyen
            var playerId = await _idCasche.GetPlayerIdAsync(userId);
            if (playerId is null or 0)
            {
                _logger.LogDebug("Logout: Player not found for UserId={UserId}", playerId);
                return;
            }
           
            await _cache.LogoutLockedRequestAsync(playerId.Value, sessionId, ct);
            _idCasche.Invalidate(userId);
            _logger.LogDebug("Logout: Cache removed for PlayerId={PlayerId}, UserId={UserId}", playerId);
        }
        /*
        public async Task<int> GetPlayerIdAsync(string userId, CancellationToken ct = default)
        {
            var playerId = await _db.Set<Player>()
                .Where(p => p.UserId == userId)
                .Select(p => p.PlayerId)
                .FirstOrDefaultAsync(ct);

            if (playerId == 0)
            {
                _logger.LogDebug("GetPlayerId: Player not found for UserId={UserId}", userId);

            }
            _logger.LogDebug("GetPlayerId: Found PlayerId={PlayerId} for UserId={UserId}", playerId, userId);
            return playerId;

        }
        */
    }
}
