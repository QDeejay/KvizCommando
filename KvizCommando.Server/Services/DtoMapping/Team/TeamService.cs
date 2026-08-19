using KvizCommando.Server.Services.PlayerCache;

namespace KvizCommando.Server.Services.DtoMapping
{
    /// <summary>
    /// A csapattal és a csapattagok képességeivel kapcsolatos műveleteket végzi.
    /// </summary>
    public partial class TeamService : ITeamService
    {
        private readonly IPlayerCacheService _cache;
        private readonly ILogger<TeamService> _logger;

        /// <summary>
        /// Létrehozza a csapatkezelő szolgáltatást.
        /// </summary>
        /// <param name="cache">A játékosállapotot kezelő gyorsítótár.</param>
        /// <param name="logger">A szolgáltatás naplózója.</param>
        public TeamService(
            IPlayerCacheService cache,
            ILogger<TeamService> logger)
        {
            _cache = cache;
            _logger = logger;
        }
    }
}
