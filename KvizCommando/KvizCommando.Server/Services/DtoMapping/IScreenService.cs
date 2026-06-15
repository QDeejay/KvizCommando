using KvizCommando.Shared.Models.Dtos;
namespace KvizCommando.Server.Services.DtoMapping
{
    public interface IScreenService
    {

        Task<HomeDTOs?> GetHomeScreenAsync(int playerId, string sessionId, CancellationToken ct = default);
        Task<TeamDtos> GetTeamScreenDataAsync(int playerId, string sessionId, CancellationToken ct = default);
        Task<QuestionDtos?> GetQuestionScreenAsync(int playerId, CancellationToken ct = default);
        Task<SoloGameDtos?> GetSoloGameScreenAsync(int playerId, string sessionId, CancellationToken ct = default);
    }
}
