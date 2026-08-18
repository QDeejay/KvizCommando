using KvizCommando.Shared.Contracts.Team;

namespace KvizCommando.Client.Features.Team.Services;

public interface ITeamClientService
{
    Task<bool> ModifySkillsAsync(
        ModifySkillRequest request,
        CancellationToken ct = default);

    Task<bool> ManageTeamAsync(
        ManageTeamRequest request,
        CancellationToken ct = default);
}
