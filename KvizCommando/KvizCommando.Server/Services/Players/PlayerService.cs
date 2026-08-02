using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Server.Services.VsGame.Match;
using KvizCommando.Server.Services.VsGame.Matchmaking;

namespace KvizCommando.Server.Services.Players
{
    public sealed class PlayerService : IPlayerService
    {
   
        private readonly IPlayerCacheService _cache;
        private readonly IUserPlayerIdCacheService _idCasche;
        private readonly IVsRankedQueueService _rankedQueue;
        private readonly IVsMatchService _vsMatch;
        private readonly ILogger<PlayerService> _logger;

        public PlayerService(
            IPlayerCacheService cache,
            ILogger<PlayerService> logger,
            IUserPlayerIdCacheService userPlayerId,
            IVsRankedQueueService rankedQueue,
            IVsMatchService vsMatch)
        {
            _cache = cache;
            _logger = logger;
            _idCasche = userPlayerId;
            _rankedQueue = rankedQueue;
            _vsMatch = vsMatch;
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

            await _rankedQueue.LeavePlayerAsync(
                playerId.Value,
                sessionId,
                ct);

            await _vsMatch.DisconnectPlayerAsync(
                playerId.Value,
                sessionId,
                ct);

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

/**
 * MÓDOSÍTÁS: logoutkor a cache lezárása előtt szerveroldalon
 * eltávolítja a játékost a VS queue-ból, futó meccs esetén pedig a
 * meglévő disconnect/bot folyamatot indítja el. Az azonosítás
 * PlayerId és SessionId alapján történik, ezért nem függ a SignalR
 * kapcsolat bontásának időzítésétől.
 *
 * A szolgáltatás a játékos kijelentkezéséhez tartozó szerveroldali
 * állapotlezárást kezeli.
 */
