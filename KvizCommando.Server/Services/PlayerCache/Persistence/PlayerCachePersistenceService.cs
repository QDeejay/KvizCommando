using System.Collections.Concurrent;
using System.Diagnostics;
using KvizCommando.Server.Services.Rankings;
using Microsoft.Extensions.Options;

namespace KvizCommando.Server.Services.PlayerCache
{
    internal sealed class PlayerCachePersistenceService : BackgroundService
    {
        private const int MIN_FLUSH_DELAY_SECONDS = 5;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly GameDbFlushService _gameDbFlush;
        private readonly IRankingCacheService _rankings;
        private readonly IOptions<PlayerCachePersistenceOptions> _options;
        private readonly ILogger<PlayerCachePersistenceService> _logger;

        private static readonly ConcurrentQueue<PlayerCachePersistenceStats> _lastScans = new();

        public PlayerCachePersistenceService(
            IServiceScopeFactory scopeFactory,
            GameDbFlushService gameDbFlush,
            IRankingCacheService rankings,
            IOptions<PlayerCachePersistenceOptions> options,
            ILogger<PlayerCachePersistenceService> logger)
        {
            _scopeFactory = scopeFactory;
            _gameDbFlush = gameDbFlush;
            _rankings = rankings;
            _options = options;
            _logger = logger;
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

                try
                {
                    await _rankings.RefreshAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Ranking snapshot refresh failed.");
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
                    TimeSpan.FromSeconds(_options.Value.FlushIntervalSeconds) - stat.Duration;
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
