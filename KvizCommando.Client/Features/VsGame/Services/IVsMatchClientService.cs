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

    /// <summary>
    /// Létrehozza a SignalR-kapcsolatot, szinkronizálja a szerveridőt, majd belépteti a játékost a rangsorolt várólistába.
    /// </summary>
    Task<VsQueueJoinResult> StartAsync(
        int classificationId,
        CancellationToken ct = default);

    /// <summary>
    /// Kilépteti a játékost az aktuális várólistából.
    /// </summary>
    Task<VsQueueLeaveStatus> LeaveQueueAsync(
        CancellationToken ct = default);

    /// <summary>
    /// A karaktert a megadott előkészítési helyhez rendeli.
    /// </summary>
    Task SelectCharacterAsync(
        int slotNumber,
        CancellationToken ct = default);

    /// <summary>
    /// A kiválasztott kérdéskategóriát a megadott körhöz rendeli.
    /// </summary>
    Task AssignLoadoutAsync(
        VsLoadoutAssignmentRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// A kiválasztott segítséget a megadott előkészítési helyhez rendeli.
    /// </summary>
    Task AssignHelpAsync(
        VsHelpAssignmentRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Törli a játékos előkészítési választásait.
    /// </summary>
    Task ResetPreparationAsync(CancellationToken ct = default);
    /// <summary>
    /// Lezárja a játékos előkészítési szakaszát.
    /// </summary>
    Task FinishPreparationAsync(CancellationToken ct = default);

    /// <summary>
    /// Kiértékelésre beküldi a becslős meccskérdés válaszát.
    /// </summary>
    Task SubmitGuessAsync(
        VsGuessAnswerRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Kiértékelésre beküldi a feleletválasztós meccskérdés válaszát.
    /// </summary>
    Task SubmitChoiceAsync(
        VsChoiceAnswerRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Felhasználja a kiválasztott segítséget az aktuális kérdésnél.
    /// </summary>
    Task UseHelpAsync(
        VsUseHelpRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Kiválasztja a kapitányi kör kérdését.
    /// </summary>
    Task SelectCaptainQuestionAsync(
        VsCaptainQuestionRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Leállítja az aktuális játékkapcsolatot.
    /// </summary>
    Task StopAsync(CancellationToken ct = default);
}
