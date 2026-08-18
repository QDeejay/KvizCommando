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
    /// <param name="classificationId">A kiválasztott rangsorolt várólista azonosítója.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A várólistára lépés eredménye és az esetleges elutasítási ok.</returns>
    Task<VsQueueJoinResult> StartAsync(
        int classificationId,
        CancellationToken ct = default);

    /// <summary>
    /// Kilépteti a játékost az aktuális várólistából.
    /// </summary>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A várólista elhagyásának állapota.</returns>
    Task<VsQueueLeaveStatus> LeaveQueueAsync(
        CancellationToken ct = default);

    /// <summary>
    /// A karaktert a megadott előkészítési helyhez rendeli.
    /// </summary>
    /// <param name="slotNumber">Az előkészítési hely egytől induló sorszáma.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task SelectCharacterAsync(
        int slotNumber,
        CancellationToken ct = default);

    /// <summary>
    /// A kiválasztott kérdéskategóriát a megadott körhöz rendeli.
    /// </summary>
    /// <param name="request">A kör és a hozzá rendelt kérdéskategória adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task AssignLoadoutAsync(
        VsLoadoutAssignmentRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// A kiválasztott segítséget a megadott előkészítési helyhez rendeli.
    /// </summary>
    /// <param name="request">Az előkészítési hely és a hozzá rendelt segítség adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task AssignHelpAsync(
        VsHelpAssignmentRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Törli a játékos előkészítési választásait.
    /// </summary>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task ResetPreparationAsync(CancellationToken ct = default);
    /// <summary>
    /// Lezárja a játékos előkészítési szakaszát.
    /// </summary>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task FinishPreparationAsync(CancellationToken ct = default);

    /// <summary>
    /// Kiértékelésre beküldi a becslős meccskérdés válaszát.
    /// </summary>
    /// <param name="request">A becslős kérdésre adott válasz és annak kliensoldali időadatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task SubmitGuessAsync(
        VsGuessAnswerRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Kiértékelésre beküldi a feleletválasztós meccskérdés válaszát.
    /// </summary>
    /// <param name="request">A feleletválasztós kérdésre adott válasz és annak kliensoldali időadatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task SubmitChoiceAsync(
        VsChoiceAnswerRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Felhasználja a kiválasztott segítséget az aktuális kérdésnél.
    /// </summary>
    /// <param name="request">Az aktuális kérdésnél felhasználandó segítség adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task UseHelpAsync(
        VsUseHelpRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Kiválasztja a kapitányi kör kérdését.
    /// </summary>
    /// <param name="request">A kapitányi körben kiválasztott kérdés adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task SelectCaptainQuestionAsync(
        VsCaptainQuestionRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Leállítja az aktuális játékkapcsolatot.
    /// </summary>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task StopAsync(CancellationToken ct = default);
}
