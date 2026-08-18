using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Contracts.Question;

namespace KvizCommando.Server.Services.Db
{
    public interface IQuestionDbService
    {
        /// <summary>
        /// Betölti a játékos kérdésadatait az adatbázisból.
        /// </summary>
        Task<CachedQuestion?> LoadQuestionsFromDbAsync(
           int playerId,
           CancellationToken ct);
        /// <summary>
        /// Elmenti a játékos módosított kérdésadatait az adatbázisba.
        /// </summary>
        Task<QuestionStats> SaveQuestionsToDbAsync(
            CachedQuestion cache,
            CancellationToken ct = default);
    }
}
