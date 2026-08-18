using KvizCommando.Server.Services.VsGame.Matchmaking;
using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Server.Services.VsGame.Match;

public interface IVsMatchService
{
    /// <summary>
    /// Lezárja a várólista kijelölt játékosait egy új meccshez.
    /// </summary>
    /// <param name="entries">A meccshez zárolt várólista-bejegyzések.</param>
    VsMatchSession LockMatch(
        IReadOnlyList<VsRankedQueueEntry> entries);

    /// <summary>
    /// Előkészíti a lezárt meccs játékosait, kérdéseit és kezdőállapotát.
    /// </summary>
    /// <param name="match">Az inicializálandó, már zárolt meccsállapot.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task<bool> InitializeLockedMatchAsync(
        VsMatchSession match,
        CancellationToken ct = default);

    /// <summary>
    /// A karaktert a megadott előkészítési helyhez rendeli.
    /// </summary>
    /// <param name="connectionId">Az aktív SignalR-kapcsolat azonosítója.</param>
    /// <param name="slotNumber">Az előkészítési hely egytől induló sorszáma.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task SelectCharacterAsync(
        string connectionId,
        int slotNumber,
        CancellationToken ct = default);

    /// <summary>
    /// A kiválasztott kérdéskategóriát a megadott körhöz rendeli.
    /// </summary>
    /// <param name="connectionId">Az aktív SignalR-kapcsolat azonosítója.</param>
    /// <param name="request">A feldolgozandó kérés adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task AssignLoadoutAsync(
        string connectionId,
        VsLoadoutAssignmentRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// A kiválasztott segítséget a megadott előkészítési helyhez rendeli.
    /// </summary>
    /// <param name="connectionId">Az aktív SignalR-kapcsolat azonosítója.</param>
    /// <param name="request">A feldolgozandó kérés adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task AssignHelpAsync(
        string connectionId,
        VsHelpAssignmentRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Törli a játékos előkészítési választásait.
    /// </summary>
    /// <param name="connectionId">Az aktív SignalR-kapcsolat azonosítója.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task ResetPreparationAsync(
        string connectionId,
        CancellationToken ct = default);

    /// <summary>
    /// Lezárja a játékos előkészítési szakaszát.
    /// </summary>
    /// <param name="connectionId">Az aktív SignalR-kapcsolat azonosítója.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task FinishPreparationAsync(
        string connectionId,
        CancellationToken ct = default);

    /// <summary>
    /// Kiértékelésre beküldi a becslős meccskérdés válaszát.
    /// </summary>
    /// <param name="connectionId">Az aktív SignalR-kapcsolat azonosítója.</param>
    /// <param name="request">A feldolgozandó kérés adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task SubmitGuessAsync(
        string connectionId,
        VsGuessAnswerRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Kiértékelésre beküldi a feleletválasztós meccskérdés válaszát.
    /// </summary>
    /// <param name="connectionId">Az aktív SignalR-kapcsolat azonosítója.</param>
    /// <param name="request">A feldolgozandó kérés adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task SubmitChoiceAsync(
        string connectionId,
        VsChoiceAnswerRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Felhasználja a kiválasztott segítséget az aktuális kérdésnél.
    /// </summary>
    /// <param name="connectionId">Az aktív SignalR-kapcsolat azonosítója.</param>
    /// <param name="request">A feldolgozandó kérés adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task UseHelpAsync(
        string connectionId,
        VsUseHelpRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Kiválasztja a kapitányi kör kérdését.
    /// </summary>
    /// <param name="connectionId">Az aktív SignalR-kapcsolat azonosítója.</param>
    /// <param name="request">A feldolgozandó kérés adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task SelectCaptainQuestionAsync(
        string connectionId,
        VsCaptainQuestionRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Feldolgozza a klienskapcsolat megszakadását.
    /// </summary>
    /// <param name="connectionId">Az aktív SignalR-kapcsolat azonosítója.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task DisconnectAsync(
        string connectionId,
        CancellationToken ct = default);

    /// <summary>
    /// Eltávolítja vagy automatikus játékra állítja a lekapcsolódott játékost.
    /// </summary>
    /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task DisconnectPlayerAsync(
        int playerId,
        CancellationToken ct = default);
}
