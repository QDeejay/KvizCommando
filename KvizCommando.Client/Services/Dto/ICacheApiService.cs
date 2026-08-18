using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.Dto
{
    public interface ICacheApiService
    {
        /// <summary>
        /// Lekéri a kezdőképernyő megjelenítéséhez szükséges adatokat.
        /// </summary>
        Task<HomeDTOs?> GetHomeScreenAsync(CancellationToken ct = default);
        /// <summary>
        /// Lekéri a kérdéskezelő képernyő adatait.
        /// </summary>
        Task<QuestionDtos?> GetQuestionAsync(CancellationToken ct = default);
        /// <summary>
        /// Lekéri a csapatképernyő adatait.
        /// </summary>
        Task<TeamDtos?> GetTeamAsync(CancellationToken ct = default);
        /// <summary>
        /// Lekéri az egyéni játék választóképernyőjének adatait.
        /// </summary>
        Task<SoloGameDtos?> GetSoloAsync(CancellationToken ct = default);
        /// <summary>
        /// Lekéri a többjátékos mód választóképernyőjének adatait.
        /// </summary>
        Task<VsGameDtos?> GetVsGameAsync(CancellationToken ct = default);

    }
}
