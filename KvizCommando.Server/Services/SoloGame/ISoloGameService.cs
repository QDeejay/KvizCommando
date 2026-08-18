using KvizCommando.Shared.Contracts.SoloGame;

namespace KvizCommando.Server.Services.SoloGame
{
    public interface ISoloGameService
    {
        /// <summary>
        /// Ellenőrzi az indítási kérést, majd létrehozza és gyorsítótárba helyezi az egyéni játékot.
        /// </summary>
        Task<SoloStartResult> StartAsync(
            int playerId,
            StartSoloGameRequest request,
            CancellationToken ct = default);

        /// <summary>
        /// Kiértékelésre beküldi az egyéni játék válaszát.
        /// </summary>
        Task<SoloAnswerResult> SubmitAnswerAsync(
            int playerId,
            Guid gameId,
            SoloAnswerDto answer,
            CancellationToken ct = default);

        /// <summary>
        /// Megszakítja az aktuális egyéni játékot.
        /// </summary>
        Task<SoloGameOperationStatus> AbandonAsync(
            int playerId,
            Guid gameId,
            string sessionId,
            CancellationToken ct = default);
    }
}
