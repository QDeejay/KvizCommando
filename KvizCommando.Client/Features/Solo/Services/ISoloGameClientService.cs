using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Client.Features.Solo.Services;

public interface ISoloGameClientService : IAsyncDisposable
{
    event Action? OnChanged;

    VsConnectionCheckResult? ConnectionCheck { get; }
    string ErrorMessageKey { get; }
    bool IsConnected { get; }

    Task<StartSoloGameResponse?> StartAsync(
        StartSoloGameRequest request,
        CancellationToken ct = default);

    Task<SoloHubAnswerResponse?> SubmitAnswerAsync(
        SoloAnswerDto answer,
        CancellationToken ct = default);

    Task<bool> AbandonAsync(
        CancellationToken ct = default);

    Task StopAsync(CancellationToken ct = default);
}
