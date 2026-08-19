using KvizCommando.Server.Services.PlayerCache;

namespace KvizCommando.Server.Services.DtoMapping
{
    /// <summary>
    /// A játékos saját kérdéseihez és kérdéshelyeihez tartozó műveleteket végzi.
    /// </summary>
    public sealed partial class QuestionService : IQuestionService
    {
        private readonly IPlayerCacheService _cache;
        private readonly ILogger<QuestionService> _logger;

        /// <summary>
        /// Létrehozza a kérdéskezelő szolgáltatást.
        /// </summary>
        /// <param name="cache">A játékos- és kérdésállapotot kezelő gyorsítótár.</param>
        /// <param name="logger">A szolgáltatás naplózója.</param>
        public QuestionService(
            IPlayerCacheService cache,
            ILogger<QuestionService> logger)
        {
            _cache = cache;
            _logger = logger;
        }
    }
}
