using KvizCommando.Shared.Contracts.SoloGame;

namespace KvizCommando.Server.Services.SoloGame
{
    public interface ISoloGameService
    {
        Task<(StartSoloGameResponse? Response, bool? Success)> StartAsync(
            int playerId,
            StartSoloGameRequest request,
            CancellationToken ct = default);

        Task<(StartSoloGameResponse? Response, bool? Success)> StartSignalRAsync(
            int playerId,
            StartSoloGameRequest request,
            CancellationToken ct = default);

        Task<(FinishSoloGameResponse? Response, bool? Success)> SubmitAnswerAsync(
            int playerId,
            Guid gameId,
            SoloAnswerDto answer,
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

/**
 * MÓDOSÍTÁS: a HTTP műveletek változatlanul megmaradnak, mellettük
 * a hub rövidebb lejáratú startot és kérdésenkénti válaszküldést kap.
 */
