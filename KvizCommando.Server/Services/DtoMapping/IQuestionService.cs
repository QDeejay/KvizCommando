using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Contracts.Question;

namespace KvizCommando.Server.Services.DtoMapping
{
    public interface IQuestionService
    {

        /// <summary>
        /// Elmenti a gyári kérdéshelyek összeállítását.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="dto">A feldolgozandó kérés adatai.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CacheUpdateResult> SaveFactorySlotsAsync(int playerId, SaveFactoryRequest dto, CancellationToken ct);
        /// <summary>
        /// Végrehajtja a kérdéshelyeken kért kezelési műveletet.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="dto">A feldolgozandó kérés adatai.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CacheUpdateResult> ManageSlotsAsync(int playerId, ManageSlotRequest dto, CancellationToken ct);
        /// <summary>
        /// Beküldi az új felhasználói kérdést ellenőrzésre.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="dto">A feldolgozandó kérés adatai.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CacheUpdateResult> SendNewQuestionAsync(int playerId, NewQuestionRequest dto, CancellationToken ct);

    }
}
