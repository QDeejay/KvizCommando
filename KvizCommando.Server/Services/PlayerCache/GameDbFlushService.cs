using System.Collections.Concurrent;
using System.Diagnostics;

namespace KvizCommando.Server.Services.PlayerCache
{
    public sealed class GameDbFlushService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private static readonly ConcurrentQueue<GameDbFlushStats> _lastFlushes = new();

        public GameDbFlushService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task FlushAsync(
            int[] playerIds,
            CancellationToken ct = default)
        {
            if (playerIds.Length == 0)
                return;

            var sw = Stopwatch.StartNew();
            var dirtyPlayers = 0;
            var totalQuestions = 0;

            using var scope = _scopeFactory.CreateScope();
            var cacheService = scope.ServiceProvider
                .GetRequiredService<IPlayerCacheService>();

            foreach (var playerId in playerIds)
            {
                var questionCount =
                    await cacheService.SaveDirtyQuestionLockedAsync(
                        playerId,
                        ct);

                if (questionCount <= 0)
                    continue;

                dirtyPlayers++;
                totalQuestions += questionCount;
            }

            sw.Stop();

            var stat = new GameDbFlushStats
            {
                Timestamp = DateTime.UtcNow,
                Duration = sw.Elapsed,
                TargetPlayers = playerIds.Length,
                DirtyPlayers = dirtyPlayers,
                SavedQuestions = totalQuestions
            };

            _lastFlushes.Enqueue(stat);
            while (_lastFlushes.Count > 10 && _lastFlushes.TryDequeue(out _)) { }

            var statsArray = _lastFlushes.ToArray();
            var avgDuration = statsArray.Any()
                ? TimeSpan.FromMilliseconds(
                    statsArray.Average(item => item.Duration.TotalMilliseconds))
                : TimeSpan.Zero;

            Console.WriteLine(
                $"[GameDbFlush] Lefutás: {stat.Duration.TotalMilliseconds:F0} ms | " +
                $"Célzott player: {stat.TargetPlayers} | " +
                $"Dirty player: {stat.DirtyPlayers} | " +
                $"Mentett kérdés: {stat.SavedQuestions} | " +
                $"Átlag: {avgDuration.TotalMilliseconds:F0} ms");
        }
    }

    public sealed class GameDbFlushStats
    {
        public DateTime Timestamp { get; init; }
        public TimeSpan Duration { get; init; }
        public int TargetPlayers { get; init; }
        public int DirtyPlayers { get; init; }
        public int SavedQuestions { get; init; }
    }
}
