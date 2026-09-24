using KvizCommando.Shared.Models.Enums;

namespace KvizCommando.Server.Services.Rankings;

internal sealed record RankingValue(
    int PlayerId,
    double Score,
    double TimeSeconds,
    string? PlaceholderName = null);

internal sealed record RankingCandidate(
    RankingValue Value,
    int Position,
    bool HasGapBefore,
    RankingValue? Previous);

internal sealed record RankingSelection(
    IReadOnlyList<RankingCandidate> Rows,
    int? CurrentPosition);

internal sealed class RankingCacheSnapshot
{
    public required Dictionary<RankingList, Dictionary<int, RankingValue>> Values { get; init; }
    public required Dictionary<RankingList, RankingValue[]> Ordered { get; init; }
    public required DateTime UpdatedUtc { get; init; }
}
