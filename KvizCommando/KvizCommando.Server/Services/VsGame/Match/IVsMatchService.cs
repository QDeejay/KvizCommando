using KvizCommando.Server.Services.VsGame.Matchmaking;
using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Server.Services.VsGame.Match;

public interface IVsMatchService
{
    VsMatchSession LockMatch(
        IReadOnlyList<VsRankedQueueEntry> entries);

    Task<bool> InitializeLockedMatchAsync(
        VsMatchSession match,
        CancellationToken ct = default);

    Task SelectCharacterAsync(
        string connectionId,
        int slotNumber,
        CancellationToken ct = default);

    Task AssignLoadoutAsync(
        string connectionId,
        VsLoadoutAssignmentRequest request,
        CancellationToken ct = default);

    Task AssignHelpAsync(
        string connectionId,
        VsHelpAssignmentRequest request,
        CancellationToken ct = default);

    Task ResetPreparationAsync(
        string connectionId,
        CancellationToken ct = default);

    Task FinishPreparationAsync(
        string connectionId,
        CancellationToken ct = default);

    Task SubmitGuessAsync(
        string connectionId,
        VsGuessAnswerRequest request,
        CancellationToken ct = default);

    Task SubmitChoiceAsync(
        string connectionId,
        VsChoiceAnswerRequest request,
        CancellationToken ct = default);

    Task UseHelpAsync(
        string connectionId,
        VsUseHelpRequest request,
        CancellationToken ct = default);

    Task SelectCaptainQuestionAsync(
        string connectionId,
        VsCaptainQuestionRequest request,
        CancellationToken ct = default);

    Task DisconnectAsync(
        string connectionId,
        CancellationToken ct = default);

    Task DisconnectPlayerAsync(
        int playerId,
        string sessionId,
        CancellationToken ct = default);
}

/**
 * MÓDOSÍTÁS: a szinkron LockMatch azonnal regisztrálja a kiválasztott
 * játékosokat, az aszinkron inicializálás csak ezután foglal tétet és
 * tölt adatot. Így nincs queue és match store közötti láthatatlan
 * disconnect-időablak.
 *
 * MÓDOSÍTÁS: felvette a három szándék szerinti játékmeneti parancsot:
 * tipp, feleletválasztós válasz és kapitánykérdés kiválasztása.
 * MÓDOSÍTÁS: a játékkörben használható segítség külön, explicit
 * szerverparancs.
 * MÓDOSÍTÁS: logoutkor PlayerId és SessionId alapján is elérhető a
 * normál disconnect/bot folyamat.
 *
 * A lezárt VS meccs létrehozásának, preparációs és játékmeneti
 * parancsainak szerveroldali szerződése.
 */
