using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Client.Features.VsGame.Services;

public interface IVsMatchClientService : IAsyncDisposable
{
    event Action? OnChanged;

    VsRankedQueueSnapshot? QueueSnapshot { get; }
    VsMatchSnapshot? MatchSnapshot { get; }
    VsConnectionCheckResult? ConnectionCheck { get; }
    string ErrorMessageKey { get; }
    bool IsConnected { get; }
    DateTime ServerUtcNow { get; }

    Task<VsQueueJoinResult> StartAsync(
        int classificationId,
        CancellationToken ct = default);

    Task<bool> LeaveQueueAsync(CancellationToken ct = default);

    Task SelectCharacterAsync(
        int slotNumber,
        CancellationToken ct = default);

    Task AssignLoadoutAsync(
        VsLoadoutAssignmentRequest request,
        CancellationToken ct = default);

    Task AssignHelpAsync(
        VsHelpAssignmentRequest request,
        CancellationToken ct = default);

    Task ResetPreparationAsync(CancellationToken ct = default);
    Task FinishPreparationAsync(CancellationToken ct = default);

    Task SubmitGuessAsync(
        VsGuessAnswerRequest request,
        CancellationToken ct = default);

    Task SubmitChoiceAsync(
        VsChoiceAnswerRequest request,
        CancellationToken ct = default);

    Task UseHelpAsync(
        VsUseHelpRequest request,
        CancellationToken ct = default);

    Task SelectCaptainQuestionAsync(
        VsCaptainQuestionRequest request,
        CancellationToken ct = default);

    Task StopAsync(CancellationToken ct = default);
}

/**
 * MÓDOSÍTÁS: a StartAsync közvetlen queue-belépési eredményt ad, a
 * játékmeneti parancsok pedig nem nyelhetik el a megszakadt kapcsolat
 * hibáját.
 *
 * MÓDOSÍTÁS: a tipp-, válasz- és kapitánykérdés-parancsot külön,
 * explicit metódusként továbbítja.
 * MÓDOSÍTÁS: a segítség használata ugyancsak explicit SignalR-
 * parancs, állapotot a kliensszerviz nem tart hozzá.
 * MÓDOSÍTÁS: a VS-időzítők számára a kapcsolat elején szinkronizált,
 * monotón módon továbbhaladó becsült szerveridőt teszi elérhetővé.
 * MÓDOSÍTÁS: az egyszeri kapcsolatellenőrzés típusos eredményét a
 * manager számára olvashatóvá teszi.
 * MÓDOSÍTÁS: a manuális queue-kilépés bool eredménye különbözteti meg
 * a tényleges várólista-elhagyást a már lockolt vagy lezárt állapottól.
 *
 * A VS dynamic manager által használt SignalR klienskapcsolat
 * szerződése. Automatikus reconnectet szándékosan nem tartalmaz.
 */
