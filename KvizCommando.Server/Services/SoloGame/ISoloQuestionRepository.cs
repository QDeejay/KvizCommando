using KvizCommando.Server.Domain.Entities.Questions;
namespace KvizCommando.Server.Services.SoloGame;
public interface ISoloQuestionRepository
{
    /// <summary>
    /// Betölti a megadott azonosítójú feleletválasztós kérdéseket.
    /// </summary>
    /// <param name="ids">A betöltendő kérdések adatbázis-azonosítói.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task<IReadOnlyList<FactoryQuestion>> LoadByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    /// <summary>
    /// Betölti a megadott azonosítójú becslős kérdéseket.
    /// </summary>
    /// <param name="ids">A betöltendő kérdések adatbázis-azonosítói.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task<IReadOnlyList<GuessQuestion>> LoadGuessByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
}
