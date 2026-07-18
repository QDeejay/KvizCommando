using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Contracts.Team;

namespace KvizCommando.Client.Services.Dto
{
    public interface IApiService
    {
        Task<bool> SaveFactorySlotsAsync(SaveFactoryRequest dto, CancellationToken ct = default);
        Task<bool> ManageSlotAsync(ManageSlotRequest dto, CancellationToken ct = default);
        Task<bool> SendNewQuestionAsync(NewQuestionRequest dto, CancellationToken ct = default);
        Task<bool> ModifyTeamAsync(ModifySkillRequest dto, CancellationToken ct = default);
        Task<bool> ManageTeamAsync(ManageTeamRequest dto, CancellationToken ct = default);
    }
}
