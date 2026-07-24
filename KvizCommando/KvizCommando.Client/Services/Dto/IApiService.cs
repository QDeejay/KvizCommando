using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Contracts.Team;

namespace KvizCommando.Client.Services.Dto
{
    public interface IApiService
    {
        Task<bool> ModifyTeamAsync(ModifySkillRequest dto, CancellationToken ct = default);
        Task<bool> ManageTeamAsync(ManageTeamRequest dto, CancellationToken ct = default);

        Task<StartSoloGameResponse?> StartSoloGameAsync(StartSoloGameRequest dto, CancellationToken ct = default);
        Task<FinishSoloGameResponse?> FinishSoloGameAsync(Guid gameId, FinishSoloGameRequest dto, CancellationToken ct = default);
        Task<bool> AbandonSoloGameAsync(Guid gameId, CancellationToken ct = default);
    }
}
