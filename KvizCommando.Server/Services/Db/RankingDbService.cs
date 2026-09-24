using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Shared.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Services.Db;

/// <summary>Beolvassa a tartósított eredményeket játékosonként összesítve.</summary>
internal sealed class RankingDbService : IRankingDbService
{
    private readonly ApplicationDbContext _db;

    /// <summary>Létrehozza a ranglista-eredmények adatbázisos olvasóját.</summary>
    public RankingDbService(ApplicationDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<Dictionary<RankingList, Dictionary<int, RankingDbValue>>> ReadAsync(
        CancellationToken ct)
    {
        var category = await _db.PlayerCategoryStats.AsNoTracking()
            .Where(stat => stat.HighScore > 0)
            .GroupBy(stat => stat.PlayerId)
            .Select(group => new
            {
                PlayerId = group.Key,
                Score = group.Sum(stat => (long)stat.HighScore),
                Time = group.Sum(stat => stat.HighScoreTime)
            })
            .ToListAsync(ct);

        var orientation = await _db.PlayerOrientStat.AsNoTracking()
            .Where(stat => stat.HighScore > 0)
            .GroupBy(stat => stat.PlayerId)
            .Select(group => new
            {
                PlayerId = group.Key,
                Score = group.Sum(stat => (long)stat.HighScore),
                Time = group.Sum(stat => stat.HighScoreTime)
            })
            .ToListAsync(ct);

        var vs = await _db.TeamStatistics.AsNoTracking()
            .Where(stat => stat.RankedHighScore > 0)
            .Select(stat => new
            {
                stat.PlayerId,
                Score = stat.RankedHighScore,
                Time = stat.RankedHighScoreTime
            })
            .ToListAsync(ct);

        var categoryValues = category.ToDictionary(
            item => item.PlayerId,
            item => new RankingDbValue(item.PlayerId, item.Score, item.Time));
        var orientationValues = orientation.ToDictionary(
            item => item.PlayerId,
            item => new RankingDbValue(item.PlayerId, item.Score, item.Time));
        var allValues = new Dictionary<int, RankingDbValue>(categoryValues);

        foreach (var item in orientationValues.Values)
        {
            if (allValues.TryGetValue(item.PlayerId, out var categoryValue))
            {
                allValues[item.PlayerId] = new RankingDbValue(
                    item.PlayerId,
                    categoryValue.Score + item.Score,
                    categoryValue.TimeSeconds + item.TimeSeconds);
            }
            else
            {
                allValues[item.PlayerId] = item;
            }
        }

        return new Dictionary<RankingList, Dictionary<int, RankingDbValue>>
        {
            [RankingList.SoloOverall] = allValues,
            [RankingList.SoloCategory] = categoryValues,
            [RankingList.SoloOrientation] = orientationValues,
            [RankingList.VsAllTime] = vs.ToDictionary(
                item => item.PlayerId,
                item => new RankingDbValue(item.PlayerId, item.Score, item.Time))
        };
    }
}
