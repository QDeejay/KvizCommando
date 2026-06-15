using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static KvizCommando.Server.Services.PlayerCache.PlayerCachePersistenceService;

namespace KvizCommando.Server.Services.PlayerCache
{
    /// <summary>
    /// Másodlagos flush szolgáltatás, amelyet a PlayerCachePersistenceService triggerel,
    /// ha bármelyik player cache-ben dirty kérdésrész található.
    /// Csak a playerhez kötött Question szekciókat menti adatbázisba.
    /// </summary>
    public sealed class QuestionCacheFlushService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private static readonly ConcurrentQueue<QuestionFlushStats> _lastScans = new();

        private static bool _isRunning = false;

        public QuestionCacheFlushService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("QuestionCacheFlushService elindult, triggerre vár...");

            while (!stoppingToken.IsCancellationRequested)
            {
                // várakozás triggerre
                await QuestionFlushTrigger.WaitForTriggerAsync(stoppingToken);

                // ha épp fut egy flush, új trigger-t eldobunk
                if (_isRunning)
                    continue;

                _isRunning = true;

                var sw = Stopwatch.StartNew();
                int totalPlayers = 0;
                int dirtyPlayers = 0;
                int totalQuestions = 0;

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var cacheService = scope.ServiceProvider.GetRequiredService<IPlayerCacheService>();

                    var ids = cacheService.GetActivePlayerIds();
                    totalPlayers = ids.Count;

                    foreach (var playerId in ids)
                    {
                        var questionCount = await cacheService.SaveDirtyQuestionLockedAsync(playerId, stoppingToken);

                        if (questionCount > 0)
                        {
                            dirtyPlayers++;
                            totalQuestions += questionCount;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[QuestionFlushService] Hiba a flush során: {ex.Message}");
                }

                sw.Stop();

                var stat = new QuestionFlushStats
                {
                    Timestamp = DateTime.UtcNow,
                    Duration = sw.Elapsed,
                    TotalPlayers = totalPlayers,
                    DirtyPlayers = dirtyPlayers,
                    SavedQuestions = totalQuestions
                };

                _lastScans.Enqueue(stat);
                while (_lastScans.Count > 10 && _lastScans.TryDequeue(out _)) { }

                var statsArray = _lastScans.ToArray();
                var avgDuration = statsArray.Any()
                    ? TimeSpan.FromMilliseconds(statsArray.Average(s => s.Duration.TotalMilliseconds))
                    : TimeSpan.Zero;

                Console.WriteLine(
                    $"[QuestionFlush] Lefutás: {stat.Duration.TotalMilliseconds:F0} ms | " +
                    $"Össz. player: {stat.TotalPlayers} | " +
                    $"Dirty player: {stat.DirtyPlayers} | " +
                    $"Mentett kérdés: {stat.SavedQuestions} | " +
                    $"Átlag: {avgDuration.TotalMilliseconds:F0} ms"
                );

                _isRunning = false;
            }
        }
    }

    /// <summary>
    /// Stat objektum a legutóbbi flush futásokhoz.
    /// </summary>
    public sealed class QuestionFlushStats
    {
        public DateTime Timestamp { get; init; }
        public TimeSpan Duration { get; init; }
        public int TotalPlayers { get; init; }
        public int DirtyPlayers { get; init; }
        public int SavedQuestions { get; init; }
    }
}
