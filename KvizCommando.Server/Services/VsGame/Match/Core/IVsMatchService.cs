using KvizCommando.Server.Services.VsGame.Matchmaking;
using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Server.Services.VsGame.Match;

public interface IVsMatchService
{
    /// <summary>
    /// Lezárja a várólista kijelölt játékosait egy új meccshez.
    /// </summary>
    VsMatchSession LockMatch(
        IReadOnlyList<VsRankedQueueEntry> entries);

    /// <summary>
    /// Előkészíti a lezárt meccs játékosait, kérdéseit és kezdőállapotát.
    /// </summary>
    Task<bool> InitializeLockedMatchAsync(
        VsMatchSession match,
        CancellationToken ct = default);

    /// <summary>
    /// A karaktert a megadott előkészítési helyhez rendeli.
    /// </summary>
    Task SelectCharacterAsync(
        string connectionId,
        int slotNumber,
        CancellationToken ct = default);

    /// <summary>
    /// A kiválasztott kérdéskategóriát a megadott körhöz rendeli.
    /// </summary>
    Task AssignLoadoutAsync(
        string connectionId,
        VsLoadoutAssignmentRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// A kiválasztott segítséget a megadott előkészítési helyhez rendeli.
    /// </summary>
    Task AssignHelpAsync(
        string connectionId,
        VsHelpAssignmentRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Törli a játékos előkészítési választásait.
    /// </summary>
    Task ResetPreparationAsync(
        string connectionId,
        CancellationToken ct = default);

    /// <summary>
    /// Lezárja a játékos előkészítési szakaszát.
    /// </summary>
    Task FinishPreparationAsync(
        string connectionId,
        CancellationToken ct = default);

    /// <summary>
    /// Kiértékelésre beküldi a becslős meccskérdés válaszát.
    /// </summary>
    Task SubmitGuessAsync(
        string connectionId,
        VsGuessAnswerRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Kiértékelésre beküldi a feleletválasztós meccskérdés válaszát.
    /// </summary>
    Task SubmitChoiceAsync(
        string connectionId,
        VsChoiceAnswerRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Felhasználja a kiválasztott segítséget az aktuális kérdésnél.
    /// </summary>
    Task UseHelpAsync(
        string connectionId,
        VsUseHelpRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Kiválasztja a kapitányi kör kérdését.
    /// </summary>
    Task SelectCaptainQuestionAsync(
        string connectionId,
        VsCaptainQuestionRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Feldolgozza a klienskapcsolat megszakadását.
    /// </summary>
    Task DisconnectAsync(
        string connectionId,
        CancellationToken ct = default);

    /// <summary>
    /// Eltávolítja vagy automatikus játékra állítja a lekapcsolódott játékost.
    /// </summary>
    Task DisconnectPlayerAsync(
        int playerId,
        CancellationToken ct = default);
}
