using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.Rankings;
using KvizCommando.Server.Services.VsGame.Matchmaking;

namespace KvizCommando.Server.Services.DtoMapping
{
    /// <summary>
    /// Összeállítja a kliens fő képernyőihez szükséges adatmodelleket.
    /// </summary>
    internal sealed partial class ScreenService : IScreenService
    {
        private readonly IPlayerCacheService _cache;
        private readonly IVsRankedQueueService _rankedQueue;
        private readonly ILogger<ScreenService> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IRankingCacheService _rankings;

        /// <summary>
        /// Létrehozza a képernyőadatokat összeállító szolgáltatást.
        /// </summary>
        /// <param name="cache">A játékosállapotot kezelő gyorsítótár.</param>
        /// <param name="rankedQueue">A rangsorolt VS várólista.</param>
        /// <param name="logger">A szolgáltatás naplózója.</param>
        /// <param name="env">A szerver futási környezete.</param>
        /// <param name="rankings">A szerver közös ranglista-cache-e.</param>
        public ScreenService(
            IPlayerCacheService cache,
            IVsRankedQueueService rankedQueue,
            ILogger<ScreenService> logger,
            IWebHostEnvironment env,
            IRankingCacheService rankings)
        {
            _cache = cache;
            _rankedQueue = rankedQueue;
            _logger = logger;
            _env = env;
            _rankings = rankings;
        }
    }
}
