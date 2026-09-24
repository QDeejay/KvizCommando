using KvizCommando.Shared.Models.Enums;

namespace KvizCommando.Server.Services.Db;

/// <summary>Játékosonként összesítve olvassa a tartósított ranglistaeredményeket.</summary>
internal interface IRankingDbService
{
    /// <summary>Beolvassa a Solo és VS listák tartósított pontszámát és idejét.</summary>
    Task<Dictionary<RankingList, Dictionary<int, RankingDbValue>>> ReadAsync(
        CancellationToken ct);
}

internal sealed record RankingDbValue(int PlayerId, double Score, double TimeSeconds);
