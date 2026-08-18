using System.Collections.Concurrent;
using System.Diagnostics;

namespace KvizCommando.Server.Services.PlayerCache
{
    public sealed class PlayerCachePersistenceService : BackgroundService
    {
        private const int FLUSH_INTERVAL_SECONDS = 15;
        private const int MIN_FLUSH_DELAY_SECONDS = 5;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly GameDbFlushService _gameDbFlush;

        private static readonly ConcurrentQueue<PlayerCachePersistenceStats> _lastScans = new();

        public PlayerCachePersistenceService(
            IServiceScopeFactory scopeFactory,
            GameDbFlushService gameDbFlush)
        {
            _scopeFactory = scopeFactory;
            _gameDbFlush = gameDbFlush;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();

                var totalUsers = 0;
                var dirtyUsers = 0;
                var logoutUsers = 0;
                var obscolatedUsers = 0;
                var gameDbDirtyPlayerIds = new List<int>();

                using (var scope = _scopeFactory.CreateScope())
                {
                    var cacheService = scope.ServiceProvider
                        .GetRequiredService<IPlayerCacheService>();

                    var playerIds = cacheService.GetActivePlayerIds();
                    totalUsers = playerIds.Count;

                    foreach (var playerId in playerIds)
                    {
                        var (result, hasDirtyQuestions) =
                            await cacheService.SaveDirtyLockedAsync(
                                playerId,
                                stoppingToken);

                        if (hasDirtyQuestions)
                            gameDbDirtyPlayerIds.Add(playerId);

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

                if (gameDbDirtyPlayerIds.Count > 0)
                {
                    await _gameDbFlush.FlushAsync(
                        gameDbDirtyPlayerIds.ToArray(),
                        stoppingToken);
                }

                sw.Stop();

                var stat = new PlayerCachePersistenceStats
                {
                    Timestamp = DateTime.UtcNow,
                    TotalUsers = totalUsers,
                    DirtyUsers = dirtyUsers,
                    DirtyQuestions = gameDbDirtyPlayerIds.Count,
                    LogoutUsers = logoutUsers,
                    ObscolatedUsers = obscolatedUsers,
                    Duration = sw.Elapsed
                };

                _lastScans.Enqueue(stat);
                while (_lastScans.Count > 10 && _lastScans.TryDequeue(out _)) { }

                var statsArray = _lastScans.ToArray();
                var avgDuration = statsArray.Any()
                    ? TimeSpan.FromMilliseconds(
                        statsArray.Average(item => item.Duration.TotalMilliseconds))
                    : TimeSpan.Zero;
                var avgUsers = statsArray.Any()
                    ? (int)statsArray.Average(item => item.TotalUsers)
                    : 0;

                Console.WriteLine(
                    $"Ciklus idő: {stat.Duration.TotalMilliseconds:F0} ms | " +
                    $"Userek száma: {stat.TotalUsers} | " +
                    $"Ebből dirty: {stat.DirtyUsers} | " +
                    $"Dirty kérdés: {stat.DirtyQuestions} | " +
                    $"Logout: {stat.LogoutUsers} | " +
                    $"Lejárt: {stat.ObscolatedUsers} | " +
                    $"Átlag: {avgDuration.TotalMilliseconds:F0} ms, {avgUsers} user");

                var remainingInterval =
                    TimeSpan.FromSeconds(FLUSH_INTERVAL_SECONDS) - stat.Duration;
                var minimumDelay =
                    TimeSpan.FromSeconds(MIN_FLUSH_DELAY_SECONDS);
                var wait = remainingInterval > minimumDelay
                    ? remainingInterval
                    : minimumDelay;

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
}
