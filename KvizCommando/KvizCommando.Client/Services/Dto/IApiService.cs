using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Contracts.Team;

namespace KvizCommando.Client.Services.Dto
{
    public interface IApiService
    {
        Task<bool> SaveFactorySlotsAsync(SaveFactoryRequest dto);
        Task<bool> ManageSlotAsync(ManageSlotRequest dto);
        Task<bool> SendNewQuestionAsync(NewQuestionRequest dto);
        Task<bool> ModifyTeamAsync(ModifySkillRequest dto);
        Task<bool> ManageTeamAsync(ManageTeamRequest dto);
    }
}
