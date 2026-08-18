using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Server.Services.PlayerCache;

namespace KvizCommando.Server.Services.DtoMapping
{
    public interface IQuestionService
    {

        Task<CacheUpdateResult> SaveFactorySlotsAsync(int playerId, SaveFactoryRequest dto, CancellationToken ct);
        Task<CacheUpdateResult> ManageSlotsAsync(int playerId, ManageSlotRequest dto, CancellationToken ct);
        Task<CacheUpdateResult> SendNewQuestionAsync(int playerId, NewQuestionRequest dto, CancellationToken ct);
        Task<QuestionDtos?> GetQuestionScreenAsync(int playerId, string sessionId, CancellationToken ct = default);
    }
}
