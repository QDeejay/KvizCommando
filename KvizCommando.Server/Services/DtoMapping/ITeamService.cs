using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Contracts.Team;

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

    }
}
