using KvizCommando.Shared.Contracts.SoloGame;

namespace KvizCommando.Client.Features.Solo.Services;

public interface ISoloGameClientService
{
    Task<StartSoloGameResponse?> StartAsync(
        StartSoloGameRequest request,
        CancellationToken ct = default);

    Task<FinishSoloGameResponse?> FinishAsync(
        Guid gameId,
        FinishSoloGameRequest request,
        CancellationToken ct = default);

    Task<bool> AbandonAsync(
        Guid gameId,
        CancellationToken ct = default);
}
