using KvizCommando.Shared.Contracts.Question;

namespace KvizCommando.Client.Features.Question.Services;

public interface IQuestionClientService
{
    /// <summary>
    /// Elmenti a gyári kérdéshelyek összeállítását.
    /// </summary>
    /// <param name="request">A gyári kérdéshelyek mentendő összeállítása.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns><see langword="true"/>, ha a művelet sikeresen befejeződött; egyébként <see langword="false"/>.</returns>
    Task<bool> SaveFactorySlotsAsync(
        SaveFactoryRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Végrehajtja a kérdéshelyen kért kezelési műveletet.
    /// </summary>
    /// <param name="request">A kérdéshelyen végrehajtandó művelet adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns><see langword="true"/>, ha a művelet sikeresen befejeződött; egyébként <see langword="false"/>.</returns>
    Task<bool> ManageSlotAsync(
        ManageSlotRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Beküldi az új felhasználói kérdést ellenőrzésre.
    /// </summary>
    /// <param name="request">Az ellenőrzésre beküldött kérdés adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns><see langword="true"/>, ha a művelet sikeresen befejeződött; egyébként <see langword="false"/>.</returns>
    Task<bool> SendNewQuestionAsync(
        NewQuestionRequest request,
        CancellationToken ct = default);
}
