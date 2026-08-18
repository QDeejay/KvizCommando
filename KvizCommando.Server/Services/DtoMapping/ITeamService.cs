using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Server.Services.PlayerCache;

namespace KvizCommando.Server.Services.DtoMapping
{
    public interface ITeamService
    {

        /// <summary>
        /// Elmenti a karakter képességpontjain végzett módosítást.
        /// </summary>
        /// <param name="playerid">A játékos adatbázis-azonosítója.</param>
        /// <param name="dto">A feldolgozandó kérés adatai.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CacheUpdateResult> SaveModifiedSkillAsync(int playerid, ModifySkillRequest dto, CancellationToken ct = default);
        /// <summary>
        /// Végrehajtja a csapaton kért kezelési műveletet.
        /// </summary>
        /// <param name="playerid">A játékos adatbázis-azonosítója.</param>
        /// <param name="dto">A feldolgozandó kérés adatai.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CacheUpdateResult> ManageTeamAsync(int playerid, ManageTeamRequest dto, CancellationToken ct = default);
        /// <summary>
        /// Lekéri a csapatképernyő megjelenítési adatait.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<TeamDtos?> GetTeamScreenDataAsync(int playerId, string sessionId, CancellationToken ct = default);
    }
}
