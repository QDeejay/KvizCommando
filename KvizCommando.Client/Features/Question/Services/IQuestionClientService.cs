using KvizCommando.Shared.Contracts.Question;

namespace KvizCommando.Client.Features.Question.Services;

public interface IQuestionClientService
{
    /// <summary>
    /// Elmenti a gyári kérdéshelyek összeállítását.
    /// </summary>
    Task<bool> SaveFactorySlotsAsync(
        SaveFactoryRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Végrehajtja a kérdéshelyen kért kezelési műveletet.
    /// </summary>
    Task<bool> ManageSlotAsync(
        ManageSlotRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Beküldi az új felhasználói kérdést ellenőrzésre.
    /// </summary>
    Task<bool> SendNewQuestionAsync(
        NewQuestionRequest request,
        CancellationToken ct = default);
}
