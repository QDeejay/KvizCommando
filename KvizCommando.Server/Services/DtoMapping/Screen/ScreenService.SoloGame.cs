using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Models;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.VsGame;
using KvizCommando.Server.Services.VsGame.Matchmaking;
using KvizCommando.Server.Utilities;
using KvizCommando.Server.Utilities.Recruit;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums;
using KvizCommando.Shared.Models.Rules;
using KvizCommando.Shared.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace KvizCommando.Server.Services.DtoMapping
{
    partial class ScreenService
    {
        /// <inheritdoc />
        public async Task<SoloGameDtos?> GetSoloGameScreenAsync(int playerId, string sessionId, CancellationToken ct = default)
        {
            var cacheResult = await _cache.GetOrLoadLockedAsync(
                playerId,
                sessionId,
                ct);

            if (cacheResult.Status == CacheReadStatus.SessionMismatch)
                return new SoloGameDtos { AccessDenied = true };

            var player = cacheResult.Player;

            if (player is null)
            {
                _logger.LogWarning("Player not found in cache. userId={UserId}", playerId);
                return null;
            }
            var mask = player.CharCatMask;
            var orientationMask =
                new bool[player.Characters.Length];

            for (var index = 0;
                 index < player.Characters.Length;
                 index++)
            {
                var character = player.Characters[index];

                if (character is null)
                    continue;

                orientationMask[index] = true;
            }
            var results = new SoloResults
            {
                OrientResults = GetOriResultFromCache(player.OrientStats),
                CategoryResults = GetCatResultFromCache(player.CategoryStats)

            };
            var enables = new SoloEnables
            {
                EnaCampaign = false,
                EnaCategory = mask.AsSpan().Contains(true),
                EnaOrient = orientationMask.AsSpan().Contains(true),
                EnaOri = orientationMask,
                EnaCat = mask.Concat(mask).ToArray(),
            };


            return new SoloGameDtos
            {
                Mask = mask,
                Results = results,
                Enables = enables
            };

        }

        private static ResultDto[] GetCatResultFromCache(List<PlayerCategoryStat> data)
        {
            int ix;
            double tempTime;
            var result = new ResultDto[data.Count + 1];
            result[0] = new ResultDto { Points = 0, Time = 0.0 };
            foreach (var d in data)
            {
                ix = Math.Min((int)d.CategoryId, 16);
                tempTime = Math.Round(d.HighScoreTime, 1);
                result[ix] = new ResultDto
                {
                    Points = d.HighScore,
                    Time = tempTime,
                    TimeStr = tempTime > 99.9 ? "+99.9" : tempTime.ToString("0.0", CultureInfo.InvariantCulture)
                };
                result[0].Points += d.HighScore;
                result[0].Time += tempTime;
            }
            return result;
        }
        private static ResultDto[] GetOriResultFromCache(List<PlayerOrientStat> data)
        {
            int ix;
            double tempTime;
            var result = new ResultDto[data.Count + 1];
            result[0] = new ResultDto { Points = 0, Time = 0.0 };
            foreach (var d in data)
            {
                ix = Math.Min((int)d.OrientId, 8);
                tempTime = Math.Round(d.HighScoreTime, 1);
                result[ix] = new ResultDto
                {
                    Points = d.HighScore,
                    Time = Math.Round(d.HighScoreTime, 1),
                    TimeStr = tempTime > 99.9 ? "+99.9" : tempTime.ToString("0.0", CultureInfo.InvariantCulture)
                };
                result[0].Points += d.HighScore;
                result[0].Time += Math.Round(d.HighScoreTime, 1);
            }
            return result;
        }
    }
}
