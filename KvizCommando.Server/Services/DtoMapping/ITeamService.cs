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
        Task<CacheUpdateResult> SaveModifiedSkillAsync(int playerid, ModifySkillRequest dto, CancellationToken ct = default);
        /// <summary>
        /// Végrehajtja a csapaton kért kezelési műveletet.
        /// </summary>
        Task<CacheUpdateResult> ManageTeamAsync(int playerid, ManageTeamRequest dto, CancellationToken ct = default);
        /// <summary>
        /// Lekéri a csapatképernyő megjelenítési adatait.
        /// </summary>
        Task<TeamDtos?> GetTeamScreenDataAsync(int playerId, string sessionId, CancellationToken ct = default);
    }
}
