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

        public async Task LogoutAndRemoveCacheAsync(string userId, string sessionId, CancellationToken ct = default)
        {
            //var sessionId = "Teszt"; // nincs jelentősége, csak egyedi legyen
            var playerId = await _idCasche.GetPlayerIdAsync(userId);
            if (playerId is null or 0)
            {
                _logger.LogDebug("Logout: Player not found for UserId={UserId}", playerId);
                return;
            }

            var logoutRequested =  await _cache.LogoutLockedRequestAsync( playerId.Value, sessionId,  ct);

            _idCasche.Invalidate(userId);

            if (logoutRequested is null)
            {
                _logger.LogWarning(
                    "Logout: Session mismatch. PlayerId={PlayerId}, UserId={UserId}, SessionId={SessionId}",
                    playerId,
                    userId,
                    sessionId);

                return;
            }

            _logger.LogDebug(
                logoutRequested.Value
                    ? "Logout: Cache logout requested. PlayerId={PlayerId}, UserId={UserId}, SessionId={SessionId}"
                    : "Logout: Player was not present in cache. PlayerId={PlayerId}, UserId={UserId}, SessionId={SessionId}",
                playerId,
                userId,
                sessionId);
        }
       
    }
}
