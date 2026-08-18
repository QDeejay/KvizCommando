using KvizCommando.Server.Domain.Entities.Questions;
namespace KvizCommando.Server.Services.SoloGame;
public interface ISoloQuestionRepository
{
    /// <summary>
    /// Betölti a megadott azonosítójú feleletválasztós kérdéseket.
    /// </summary>
    Task<IReadOnlyList<FactoryQuestion>> LoadByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    /// <summary>
    /// Betölti a megadott azonosítójú becslős kérdéseket.
    /// </summary>
    Task<IReadOnlyList<GuessQuestion>> LoadGuessByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
}
