using KvizCommando.Server.Domain.Entities.Questions;
namespace KvizCommando.Server.Services.SoloGame;
public interface ISoloQuestionRepository
{
    Task<IReadOnlyList<FactoryQuestion>> LoadByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
}
