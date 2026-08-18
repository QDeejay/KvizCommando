using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Contracts.Question;

namespace KvizCommando.Server.Services.Db
{
    public interface IQuestionDbService
    {
        Task<CachedQuestion?> LoadQuestionsFromDbAsync(
           int playerId,
           CancellationToken ct);
        Task<QuestionStats> SaveQuestionsToDbAsync(
            CachedQuestion cache,
            CancellationToken ct = default);
    }
}
