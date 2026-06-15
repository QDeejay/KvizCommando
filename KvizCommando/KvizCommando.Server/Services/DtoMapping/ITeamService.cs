using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Server.Services.DtoMapping
{
    public interface ITeamService
    {
        
        Task<bool> SaveModifiedSkillAsync(int playerid, ModifySkillRequest dto, CancellationToken ct = default);
        Task<bool> ManageTeamAsync(int playerid, ManageTeamRequest dto, CancellationToken ct = default);
    }
}
