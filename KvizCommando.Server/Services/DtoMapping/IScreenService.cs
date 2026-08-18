using KvizCommando.Shared.Models.Dtos;
namespace KvizCommando.Server.Services.DtoMapping
{
    public interface IScreenService
    {

        /// <summary>
        /// Lekéri a kezdőképernyő megjelenítéséhez szükséges adatokat.
        /// </summary>
        Task<HomeDTOs?> GetHomeScreenAsync(int playerId, string sessionId, CancellationToken ct = default);

        /// <summary>
        /// Lekéri az egyéni játék választóképernyőjének adatait.
        /// </summary>
        Task<SoloGameDtos?> GetSoloGameScreenAsync(int playerId, string sessionId, CancellationToken ct = default);

        /// <summary>
        /// Lekéri a többjátékos mód választóképernyőjének adatait.
        /// </summary>
        Task<VsGameDtos?> GetVsGameScreenAsync(int playerId, string sessionId, CancellationToken ct = default);
    }
}
