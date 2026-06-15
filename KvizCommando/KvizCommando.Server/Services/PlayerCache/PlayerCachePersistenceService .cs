using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace KvizCommando.Server.Services.PlayerCache
{
    public sealed class PlayerCachePersistenceService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private static readonly ConcurrentQueue<PlayerCachePersistenceStats> _lastScans = new();

        public PlayerCachePersistenceService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();

                int totalUsers = 0;
                int dirtyUsers = 0;
                int dirtyQuestions = 0;
                int logoutUsers = 0;
                int obscolatedUsers = 0;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var cacheService = scope.ServiceProvider.GetRequiredService<IPlayerCacheService>();

                    var ids = cacheService.GetActivePlayerIds();
                    totalUsers = ids.Count;

                    foreach (var playerId in ids)
                    {
                        var (result, dirtQ) = await cacheService.SaveDirtyLockedAsync(playerId, stoppingToken);

                        if (dirtQ) dirtyQuestions++;
                        switch (result)
                        {
                            case SaveResult.Dirty:
                                dirtyUsers++;
                                break;
                            case SaveResult.Logout:
                                logoutUsers++;
                                break;
                            case SaveResult.Obscolated: 
                                obscolatedUsers++;
                                break;
                        }
                    }
                }

                sw.Stop();
                if(dirtyQuestions > 0)
                {
                    await QuestionFlushTrigger.NotifyAsync(stoppingToken);
                }

                var stat = new PlayerCachePersistenceStats
                {
                    Timestamp = DateTime.UtcNow,
                    TotalUsers = totalUsers,
                    DirtyUsers = dirtyUsers,
                    DirtyQuestions = dirtyQuestions,
                    LogoutUsers = logoutUsers,
                    ObscolatedUsers = obscolatedUsers,
                    Duration = sw.Elapsed
                };

                _lastScans.Enqueue(stat);
                while (_lastScans.Count > 10 && _lastScans.TryDequeue(out _)) { }

                var statsArray = _lastScans.ToArray();
                var avgDuration = statsArray.Any()
                    ? TimeSpan.FromMilliseconds(statsArray.Average(s => s.Duration.TotalMilliseconds))
                    : TimeSpan.Zero;
                var avgUsers = statsArray.Any()
                    ? (int)statsArray.Average(s => s.TotalUsers)
                    : 0;

                Console.WriteLine(
                    $"Ciklus idő: {stat.Duration.TotalMilliseconds:F0} ms | " +
                    $"Userek száma: {stat.TotalUsers} | " +
                    $"Ebből dirty: {stat.DirtyUsers} | " +
                    $"Dirty kérdés: {stat.DirtyQuestions} | " +
                    $"Logout: {stat.LogoutUsers} | " +
                    $"Lejárt: {stat.ObscolatedUsers} | "+
                    $"Átlag: {avgDuration.TotalMilliseconds:F0} ms, {avgUsers} user"
                );

                var wait = stat.Duration.TotalSeconds <= 10
                    ? TimeSpan.FromSeconds(15) - stat.Duration
                    : TimeSpan.FromSeconds(5);

                if (wait > TimeSpan.Zero)
                {
                    try
                    {
                        await Task.Delay(wait, stoppingToken);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// Egyszerű, biztonságos, nem duplikálódó trigger mechanizmus a QuestionCacheFlushService számára.
        /// </summary>
        public static class QuestionFlushTrigger
        {
            // Kapacitás: 1 => ha már van várakozó trigger, a következőt eldobja (nem duplikál).
            private static readonly Channel<bool> _channel = Channel.CreateBounded<bool>(
                new BoundedChannelOptions(1)
                {
                    FullMode = BoundedChannelFullMode.DropOldest,
                    SingleReader = true,
                    SingleWriter = false
                });

            private static int _pending = 0;

            /// <summary>
            /// Trigger küldése a QuestionFlushService felé. Ha már vár trigger, nem duplikálja.
            /// </summary>
            public static async Task NotifyAsync(CancellationToken token = default)
            {
                // Csak akkor írunk, ha még nincs várakozó trigger
                if (Interlocked.CompareExchange(ref _pending, 1, 0) == 0)
                {
                    try
                    {
                        await _channel.Writer.WriteAsync(true, token);
                    }
                    catch
                    {
                        // Ha valami miatt a csatorna lezárt, újraindulásig ignoráljuk
                    }
                }
            }

            /// <summary>
            /// A QuestionCacheFlushService ezzel várakozik új triggerre.
            /// </summary>
            public static async Task WaitForTriggerAsync(CancellationToken token)
            {
                await _channel.Reader.ReadAsync(token);
                Interlocked.Exchange(ref _pending, 0);
            }
        }
    }
}
