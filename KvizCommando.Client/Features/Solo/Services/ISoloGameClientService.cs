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
    Task<StartSoloGameResponse?> StartAsync(
        StartSoloGameRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Kiértékelésre beküldi az egyéni játék válaszát.
    /// </summary>
    Task<SoloHubAnswerResponse?> SubmitAnswerAsync(
        SoloAnswerDto answer,
        CancellationToken ct = default);

    /// <summary>
    /// Megszakítja az aktuális egyéni játékot.
    /// </summary>
    Task<bool> AbandonAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Leállítja az aktuális játékkapcsolatot.
    /// </summary>
    Task StopAsync(CancellationToken ct = default);
}
