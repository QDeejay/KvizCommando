using KvizCommando.Shared.Models.Dtos;
namespace KvizCommando.Server.Services.DtoMapping
{
    public interface IScreenService
    {

        /// <summary>
        /// Lekéri a kezdőképernyő megjelenítéséhez szükséges adatokat.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<HomeDTOs?> GetHomeScreenAsync(int playerId, string sessionId, CancellationToken ct = default);

        /// <summary>
        /// Lekéri az egyéni játék választóképernyőjének adatait.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<SoloGameDtos?> GetSoloGameScreenAsync(int playerId, string sessionId, CancellationToken ct = default);

        /// <summary>
        /// Lekéri a többjátékos mód választóképernyőjének adatait.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<VsGameDtos?> GetVsGameScreenAsync(int playerId, string sessionId, CancellationToken ct = default);
    }
}
