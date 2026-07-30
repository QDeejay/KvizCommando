using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Client.Features.VsGame.Match.Services;

public interface IVsMatchClientService : IAsyncDisposable
{
    event Action? OnChanged;

    VsRankedQueueSnapshot? QueueSnapshot { get; }
    VsMatchSnapshot? MatchSnapshot { get; }
    string ErrorMessageKey { get; }
    bool IsConnected { get; }

    Task<VsQueueJoinResult> StartAsync(
        int classificationId,
        CancellationToken ct = default);

    Task LeaveQueueAsync(CancellationToken ct = default);

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
 *
 * A VS dynamic manager által használt SignalR klienskapcsolat
 * szerződése. Automatikus reconnectet szándékosan nem tartalmaz.
 */
