using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.Dto
{
    public interface IScreenApiService
    {
        Task<HomeDTOs?> GetHomeScreenAsync();
        Task<QuestionDtos?> GetQuestionAsync();
        Task<TeamDtos?> GetTeamAsync();
        Task<SoloGameDtos?> GetSoloAsync();

    }
}
