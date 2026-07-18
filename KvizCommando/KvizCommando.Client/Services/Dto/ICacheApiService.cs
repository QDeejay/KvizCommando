using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.Dto
{
    public interface ICacheApiService
    {
        Task<HomeDTOs?> GetHomeScreenAsync(CancellationToken ct = default);
        Task<QuestionDtos?> GetQuestionAsync(CancellationToken ct = default);
        Task<TeamDtos?> GetTeamAsync(CancellationToken ct = default);
        Task<SoloGameDtos?> GetSoloAsync(CancellationToken ct = default);

    }
}
