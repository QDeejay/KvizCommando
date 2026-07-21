using KvizCommando.Shared.Contracts.SoloGame;

namespace KvizCommando.Server.Services.SoloGame
{
    public interface ISoloGameService
    {
        Task<(StartSoloGameResponse? Response, bool? Success)> StartAsync(
            int playerId,
            StartSoloGameRequest request,
            CancellationToken ct = default);

        Task<(FinishSoloGameResponse? Response, bool? Success)> FinishAsync(
            int playerId,
            Guid gameId,
            FinishSoloGameRequest request,
            CancellationToken ct = default);

        Task<bool?> AbandonAsync(
            int playerId,
            Guid gameId,
            string sessionId,
            CancellationToken ct = default);
    }
}
