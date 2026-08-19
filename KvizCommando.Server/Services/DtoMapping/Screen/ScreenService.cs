using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.VsGame.Matchmaking;

namespace KvizCommando.Server.Services.DtoMapping
{
    /// <summary>
    /// Összeállítja a kliens fő képernyőihez szükséges adatmodelleket.
    /// </summary>
    public sealed partial class ScreenService : IScreenService
    {
        private readonly IPlayerCacheService _cache;
        private readonly IVsRankedQueueService _rankedQueue;
        private readonly ILogger<ScreenService> _logger;
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Létrehozza a képernyőadatokat összeállító szolgáltatást.
        /// </summary>
        /// <param name="cache">A játékosállapotot kezelő gyorsítótár.</param>
        /// <param name="rankedQueue">A rangsorolt VS várólista.</param>
        /// <param name="logger">A szolgáltatás naplózója.</param>
        /// <param name="env">A szerver futási környezete.</param>
        public ScreenService(
            IPlayerCacheService cache,
            IVsRankedQueueService rankedQueue,
            ILogger<ScreenService> logger,
            IWebHostEnvironment env)
        {
            _cache = cache;
            _rankedQueue = rankedQueue;
            _logger = logger;
            _env = env;
        }
    }
}
