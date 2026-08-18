using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Contracts.Question;

namespace KvizCommando.Server.Services.Db
{
    public interface IQuestionDbService
    {
        /// <summary>
        /// Betölti a játékos kérdésadatait az adatbázisból.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CachedQuestion?> LoadQuestionsFromDbAsync(
           int playerId,
           CancellationToken ct);
        /// <summary>
        /// Elmenti a játékos módosított kérdésadatait az adatbázisba.
        /// </summary>
        /// <param name="cache">A mentendő gyorsítótárazott kérdésállapot.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<QuestionStats> SaveQuestionsToDbAsync(
            CachedQuestion cache,
            CancellationToken ct = default);
    }
}
