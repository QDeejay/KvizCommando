using KvizCommando.Shared.Contracts.SoloGame;

namespace KvizCommando.Server.Services.SoloGame
{
    public interface ISoloGameService
    {
        Task<SoloStartResult> StartAsync(
            int playerId,
            StartSoloGameRequest request,
            CancellationToken ct = default);

        Task<SoloAnswerResult> SubmitAnswerAsync(
            int playerId,
            Guid gameId,
            SoloAnswerDto answer,
            CancellationToken ct = default);

        Task<SoloGameOperationStatus> AbandonAsync(
            int playerId,
            Guid gameId,
            string sessionId,
            CancellationToken ct = default);
    }
}
