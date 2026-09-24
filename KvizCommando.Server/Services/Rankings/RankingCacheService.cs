using KvizCommando.Server.Services.Db;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums;
using Microsoft.Extensions.Options;

namespace KvizCommando.Server.Services.Rankings;

/// <summary>A négy közös ranglista pillanatképét és a játékosnak szánt válaszokat kezeli.</summary>
internal sealed class RankingCacheService : IRankingCacheService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<PlayerCachePersistenceOptions> _options;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private RankingCacheSnapshot? _snapshot;

    private static readonly IComparer<RankingValue> Order =
        Comparer<RankingValue>.Create((left, right) =>
        {
            var score = right.Score.CompareTo(left.Score);
            if (score != 0) return score;
            var time = left.TimeSeconds.CompareTo(right.TimeSeconds);
            return time != 0 ? time : left.PlayerId.CompareTo(right.PlayerId);
        });

    /// <summary>Létrehozza a közös ranglisták gyorsítótárát.</summary>
    /// <param name="scopeFactory">Az adatbázis-lekérdezésekhez használt scope.</param>
    /// <param name="options">A mentési ciklus beállításai.</param>
    public RankingCacheService(
        IServiceScopeFactory scopeFactory,
        IOptions<PlayerCachePersistenceOptions> options)
    {
        _scopeFactory = scopeFactory;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<RankingDtos> GetForPlayerAsync(
        int playerId,
        IReadOnlyDictionary<RankingList, RankingValue> currentValues,
        CancellationToken ct = default)
    {
        var snapshot = Volatile.Read(ref _snapshot) ??
            throw new InvalidOperationException("Ranking cache has not been initialized.");
        var selections = new Dictionary<RankingList, RankingSelection>();
        var hasNewerResult = false;

        foreach (var list in Enum.GetValues<RankingList>())
        {
            var currentValue = currentValues[list];
            var storedValue = snapshot.Values[list].GetValueOrDefault(playerId);
            var differsFromSnapshot = storedValue is null
                ? currentValue.Score > 0
                : storedValue.Score != currentValue.Score ||
                  storedValue.TimeSeconds != currentValue.TimeSeconds;

            hasNewerResult |= differsFromSnapshot;
            selections.Add(list, SelectRows(
                snapshot, list, playerId, currentValue, differsFromSnapshot));
        }

        var playerIds = selections.Values
            .SelectMany(selection => selection.Rows)
            .Select(row => row.Value.PlayerId)
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        IReadOnlyDictionary<int, (string DisplayName, int RankEnum)> players =
            new Dictionary<int, (string DisplayName, int RankEnum)>();

        if (playerIds.Length > 0)
        {
            using var scope = _scopeFactory.CreateScope();
            var playerDb = scope.ServiceProvider.GetRequiredService<IPlayerDbService>();
            players = await playerDb.GetRankingPlayerDetailsAsync(playerIds, ct);
        }

        return new RankingDtos
        {
            NextRefreshUtc = hasNewerResult
                ? snapshot.UpdatedUtc.AddSeconds(_options.Value.FlushIntervalSeconds + 2)
                : null,
            SoloOverall = MapRows(selections[RankingList.SoloOverall], playerId, players),
            SoloCategory = MapRows(selections[RankingList.SoloCategory], playerId, players),
            SoloOrientation = MapRows(selections[RankingList.SoloOrientation], playerId, players),
            VsAllTime = MapRows(selections[RankingList.VsAllTime], playerId, players)
        };
    }

    /// <inheritdoc />
    public async Task RefreshAsync(CancellationToken ct = default)
    {
        await _refreshLock.WaitAsync(ct);
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IRankingDbService>();
            var stored = await db.ReadAsync(ct);
            var values = new Dictionary<RankingList, Dictionary<int, RankingValue>>();
            var oldSnapshot = Volatile.Read(ref _snapshot);
            var ordered = new Dictionary<RankingList, RankingValue[]>();

            foreach (var (list, entries) in stored)
            {
                var currentValues = entries.ToDictionary(
                    entry => entry.Key,
                    entry => new RankingValue(
                        entry.Value.PlayerId,
                        entry.Value.Score,
                        entry.Value.TimeSeconds));
                values[list] = currentValues;

                if (oldSnapshot is not null &&
                    SameValues(oldSnapshot.Values[list], currentValues))
                {
                    ordered[list] = oldSnapshot.Ordered[list];
                    continue;
                }

                var rows = currentValues.Values.Concat(RankingDefaults.Players).ToArray();
                Array.Sort(rows, Order);
                ordered[list] = rows;
            }

            Volatile.Write(ref _snapshot, new RankingCacheSnapshot
            {
                Values = values,
                Ordered = ordered,
                UpdatedUtc = DateTime.UtcNow
            });
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static RankingSelection SelectRows(
        RankingCacheSnapshot snapshot,
        RankingList list,
        int playerId,
        RankingValue currentValue,
        bool differsFromSnapshot)
    {
        var ordered = snapshot.Ordered[list];

        if (differsFromSnapshot)
        {
            var adjusted = ordered.Where(row => row.PlayerId != playerId).ToList();
            if (currentValue.Score > 0)
            {
                var position = adjusted.BinarySearch(currentValue, Order);
                if (position < 0) position = ~position;
                adjusted.Insert(position, currentValue);
            }
            ordered = adjusted.ToArray();
        }

        var playerIndex = Array.FindIndex(
            ordered, row => row.PlayerId == playerId);
        var indices = new List<int>();

        for (var index = 0; index < Math.Min(10, ordered.Length); index++)
            indices.Add(index);

        if (playerIndex >= 10)
        {
            if (playerIndex > 10)
                indices.Add(playerIndex - 1);
            indices.Add(playerIndex);
        }

        var rows = indices.Select(index => new RankingCandidate(
            ordered[index],
            index + 1,
            playerIndex > 10 && index == playerIndex - 1,
            index > 0 ? ordered[index - 1] : null)).ToList();

        return new RankingSelection(
            rows, playerIndex < 0 ? null : playerIndex + 1);
    }

    private static RankingListDto MapRows(
        RankingSelection selection,
        int playerId,
        IReadOnlyDictionary<int, (string DisplayName, int RankEnum)> players)
    {
        var result = new RankingListDto { CurrentPosition = selection.CurrentPosition };
        var rows = new List<RankingRowDto>();

        foreach (var row in selection.Rows)
        {
            var value = row.Value;
            var isCurrent = value.PlayerId == playerId;
            var isPlaceholder = value.PlaceholderName is not null;
            if (!isPlaceholder && !players.TryGetValue(value.PlayerId, out _))
                continue;

            var name = isPlaceholder ? value.PlaceholderName! :
                players[value.PlayerId].DisplayName;
            var rank = isPlaceholder ? 0 :
                players[value.PlayerId].RankEnum;

            double? difference = null;
            if (row.Previous is not null &&
                row.Previous.Score == value.Score &&
                value.TimeSeconds > row.Previous.TimeSeconds)
            {
                difference = value.TimeSeconds - row.Previous.TimeSeconds;
            }

            rows.Add(new RankingRowDto
            {
                Position = row.Position,
                DisplayName = name,
                RankEnum = rank,
                Score = value.Score,
                TimeDifferenceSeconds = difference,
                IsCurrentPlayer = isCurrent,
                HasGapBefore = row.HasGapBefore
            });
        }

        result.Rows = rows.ToArray();
        return result;
    }

    private static bool SameValues(
        Dictionary<int, RankingValue> previous,
        Dictionary<int, RankingValue> current) =>
        previous.Count == current.Count &&
        current.All(item => previous.TryGetValue(item.Key, out var old) &&
                            old == item.Value);
}
