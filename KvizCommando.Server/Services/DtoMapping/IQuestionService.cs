using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Server.Services.PlayerCache;

namespace KvizCommando.Server.Services.DtoMapping
{
    public interface IQuestionService
    {

        /// <summary>
        /// Elmenti a gyári kérdéshelyek összeállítását.
        /// </summary>
        Task<CacheUpdateResult> SaveFactorySlotsAsync(int playerId, SaveFactoryRequest dto, CancellationToken ct);
        /// <summary>
        /// Végrehajtja a kérdéshelyeken kért kezelési műveletet.
        /// </summary>
        Task<CacheUpdateResult> ManageSlotsAsync(int playerId, ManageSlotRequest dto, CancellationToken ct);
        /// <summary>
        /// Beküldi az új felhasználói kérdést ellenőrzésre.
        /// </summary>
        Task<CacheUpdateResult> SendNewQuestionAsync(int playerId, NewQuestionRequest dto, CancellationToken ct);
        /// <summary>
        /// Lekéri a kérdéskezelő képernyő megjelenítési adatait.
        /// </summary>
        Task<QuestionDtos?> GetQuestionScreenAsync(int playerId, string sessionId, CancellationToken ct = default);
    }
}
