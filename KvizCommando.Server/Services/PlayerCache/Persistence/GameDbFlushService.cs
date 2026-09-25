using System.Collections.Concurrent;
using System.Diagnostics;
using KvizCommando.Server.Services.Db;

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

        /// <summary>
        /// A megadott játékosok módosított cache-adatait tartós tárba írja.
        /// </summary>
        /// <param name="playerIds">A tartós tárba írandó játékosok azonosítói.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
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
            var factory = new Dictionary<int, int>();
            var guess = new Dictionary<int, int>();
            var user = new Dictionary<int, int>();

            foreach (var playerId in playerIds)
            {
                var reports = await cacheService.TakeReportedQuestionsLockedAsync(playerId, ct);
                AddReports(factory, reports.FactoryIds);
                AddReports(guess, reports.GuessIds);
                AddReports(user, reports.UserIds);
                var questionCount =
                    await cacheService.SaveDirtyQuestionLockedAsync(
                        playerId,
                        ct);

                if (questionCount <= 0)
                    continue;

                dirtyPlayers++;
                totalQuestions += questionCount;
            }

            await scope.ServiceProvider.GetRequiredService<IQuestionDbService>()
                .IncrementReportedAsync(factory, guess, user, ct);

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

        private static void AddReports(Dictionary<int, int> counts, int[] ids)
        {
            foreach (var id in ids)
                counts[id] = counts.GetValueOrDefault(id) + 1;
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
