using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Contracts.Team;

namespace KvizCommando.Client.Services.Dto
{
    public interface IApiService
    {
        Task<(bool Success, string Message)> SaveFactorySlotsAsync(SaveFactoryRequest dto);
        Task<(bool Success, string Message)> ManageSlotAsync(ManageSlotRequest dto);
        Task<(bool Success, string Message)> SendNewQuestionAsync(NewQuestionRequest dto);
        Task<(bool Success, string Message)> ModifyTeamAsync(ModifySkillRequest dto);
        Task<(bool Success, string Message)> ManageTeamAsync(ManageTeamRequest dto);
    }
}
