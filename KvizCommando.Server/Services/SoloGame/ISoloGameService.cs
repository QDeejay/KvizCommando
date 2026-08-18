using KvizCommando.Shared.Contracts.SoloGame;

namespace KvizCommando.Server.Services.SoloGame
{
    public interface ISoloGameService
    {
        /// <summary>
        /// Ellenőrzi az indítási kérést, majd létrehozza és gyorsítótárba helyezi az egyéni játékot.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="request">A feldolgozandó kérés adatai.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<SoloStartResult> StartAsync(
            int playerId,
            StartSoloGameRequest request,
            CancellationToken ct = default);

        /// <summary>
        /// Kiértékelésre beküldi az egyéni játék válaszát.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="gameId">Az aktív egyéni játék azonosítója.</param>
        /// <param name="answer">A kiértékelendő válasz és annak kliensoldali időadatai.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<SoloAnswerResult> SubmitAnswerAsync(
            int playerId,
            Guid gameId,
            SoloAnswerDto answer,
            CancellationToken ct = default);

        /// <summary>
        /// Megszakítja az aktuális egyéni játékot.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="gameId">Az aktív egyéni játék azonosítója.</param>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<SoloGameOperationStatus> AbandonAsync(
            int playerId,
            Guid gameId,
            string sessionId,
            CancellationToken ct = default);
    }
}
