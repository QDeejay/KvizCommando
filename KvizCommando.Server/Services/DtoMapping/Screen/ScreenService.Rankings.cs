using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.Rankings;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums;

namespace KvizCommando.Server.Services.DtoMapping
{
    partial class ScreenService
    {
        /// <inheritdoc />
        public async Task<RankingDtos?> GetRankingScreenAsync(
            int playerId,
            string sessionId,
            CancellationToken ct)
        {
            var cacheResult = await _cache.GetOrLoadLockedAsync(
                playerId, sessionId, ct);

            if (cacheResult.Status == CacheReadStatus.SessionMismatch)
                return new RankingDtos { AccessDenied = true };
            if (cacheResult.Player is null)
                return null;

            return await _rankings.GetForPlayerAsync(
                playerId, GetPlayerValues(cacheResult.Player), ct);
        }

        private static Dictionary<RankingList, RankingValue> GetPlayerValues(
            CachedPlayer player)
        {
            var categories = player.CategoryStats.Where(stat => stat.HighScore > 0);
            var orientations = player.OrientStats.Where(stat => stat.HighScore > 0);
            var categoryScore = categories.Sum(stat => (long)stat.HighScore);
            var categoryTime = categories.Sum(stat => stat.HighScoreTime);
            var orientationScore = orientations.Sum(stat => (long)stat.HighScore);
            var orientationTime = orientations.Sum(stat => stat.HighScoreTime);
            var playerId = player.Core.PlayerId;

            return new Dictionary<RankingList, RankingValue>
            {
                [RankingList.SoloOverall] = new(
                    playerId,
                    categoryScore + orientationScore,
                    categoryTime + orientationTime),
                [RankingList.SoloCategory] = new(
                    playerId, categoryScore, categoryTime),
                [RankingList.SoloOrientation] = new(
                    playerId, orientationScore, orientationTime),
                [RankingList.VsAllTime] = new(
                    playerId,
                    player.TeamStats.RankedHighScore,
                    player.TeamStats.RankedHighScoreTime)
            };
        }
    }
}
