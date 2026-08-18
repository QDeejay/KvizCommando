using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Server.Services.PlayerCache;

namespace KvizCommando.Server.Services.DtoMapping
{
    public interface ITeamService
    {

        Task<CacheUpdateResult> SaveModifiedSkillAsync(int playerid, ModifySkillRequest dto, CancellationToken ct = default);
        Task<CacheUpdateResult> ManageTeamAsync(int playerid, ManageTeamRequest dto, CancellationToken ct = default);
        Task<TeamDtos?> GetTeamScreenDataAsync(int playerId, string sessionId, CancellationToken ct = default);
    }
}
