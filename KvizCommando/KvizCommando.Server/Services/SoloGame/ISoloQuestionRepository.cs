using KvizCommando.Server.Domain.Entities.Questions;
namespace KvizCommando.Server.Services.SoloGame;
public interface ISoloQuestionRepository
{
    Task<IReadOnlyList<FactoryQuestion>> LoadByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<IReadOnlyList<GuessQuestion>> LoadGuessByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
}

/**
 * MÓDOSÍTÁS: a gyári feleletválasztós kérdések mellett az explicit
 * tippkérdés-ID-k kötegelt, read-only betöltését is szerződésbe emeli.
 */
