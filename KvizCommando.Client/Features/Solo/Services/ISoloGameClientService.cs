using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Client.Features.Solo.Services;

public interface ISoloGameClientService : IAsyncDisposable
{
    event Action? OnChanged;

    VsConnectionCheckResult? ConnectionCheck { get; }
    string ErrorMessageKey { get; }
    bool IsConnected { get; }

    /// <summary>
    /// Létrehozza a SignalR-kapcsolatot, ellenőrzi annak minőségét, majd elindítja az egyéni játékot.
    /// </summary>
    /// <param name="request">Az egyéni játék módját, választását és munkamenetét tartalmazó kérés.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>Az elindított játék kliensállapota, vagy <see langword="null"/>, ha a kapcsolat vagy az indítás sikertelen.</returns>
    Task<StartSoloGameResponse?> StartAsync(
        StartSoloGameRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Kiértékelésre beküldi az egyéni játék válaszát.
    /// </summary>
    /// <param name="answer">A kérdés azonosítója, a választott válasz és a kliensen mért válaszidő.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A válasz szerveroldali értékelése, vagy <see langword="null"/>, ha a kapcsolat megszakadt.</returns>
    Task<SoloHubAnswerResponse?> SubmitAnswerAsync(
        SoloAnswerDto answer,
        CancellationToken ct = default);

    /// <summary>
    /// Megszakítja az aktuális egyéni játékot.
    /// </summary>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns><see langword="true"/>, ha a művelet sikeresen befejeződött; egyébként <see langword="false"/>.</returns>
    Task<bool> AbandonAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Leállítja az aktuális játékkapcsolatot.
    /// </summary>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task StopAsync(CancellationToken ct = default);
}
