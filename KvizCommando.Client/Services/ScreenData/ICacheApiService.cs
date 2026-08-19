using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.ScreenData
{
    /// <summary>
    /// Lekéri a fő kliensképernyők szerveroldalon összeállított adatait.
    /// </summary>
    public interface ICacheApiService
    {
        /// <summary>
        /// Lekéri a kezdőképernyő megjelenítéséhez szükséges adatokat.
        /// </summary>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        /// <returns>A kezdőképernyő állapotpillanatképe, vagy <see langword="null"/>, ha nem tölthető be.</returns>
        Task<HomeDTOs?> GetHomeScreenAsync(CancellationToken ct = default);
        /// <summary>
        /// Lekéri a kérdéskezelő képernyő adatait.
        /// </summary>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        /// <returns>A kérdéskezelő képernyő állapotpillanatképe, vagy <see langword="null"/>, ha nem tölthető be.</returns>
        Task<QuestionDtos?> GetQuestionAsync(CancellationToken ct = default);
        /// <summary>
        /// Lekéri a csapatképernyő adatait.
        /// </summary>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        /// <returns>A csapatképernyő állapotpillanatképe, vagy <see langword="null"/>, ha nem tölthető be.</returns>
        Task<TeamDtos?> GetTeamAsync(CancellationToken ct = default);
        /// <summary>
        /// Lekéri az egyéni játék választóképernyőjének adatait.
        /// </summary>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        /// <returns>Az egyéni játék választóképernyőjének állapota, vagy <see langword="null"/>, ha nem tölthető be.</returns>
        Task<SoloGameDtos?> GetSoloAsync(CancellationToken ct = default);
        /// <summary>
        /// Lekéri a többjátékos mód választóképernyőjének adatait.
        /// </summary>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        /// <returns>A többjátékos választóképernyő állapota, vagy <see langword="null"/>, ha nem tölthető be.</returns>
        Task<VsGameDtos?> GetVsGameAsync(CancellationToken ct = default);

    }
}
