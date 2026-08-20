namespace KvizCommando.Server.Services.PlayerCache
{
    public interface IPlayerCacheService
    {

        /// <summary>
        /// Visszaadja a gyorsítótárban jelenleg aktív játékosok azonosítóit.
        /// </summary>
        IReadOnlyCollection<int> GetActivePlayerIds();
        /// <summary>
        /// Ellenőrzi a cache-ben élő, akár még ki nem írt normalizált csapatneveket.
        /// </summary>
        bool IsNormalizedTeamNameInUse(
            string normalizedTeamName,
            int excludedPlayerId);
        /// <summary>
        /// A játékoslock alatt visszaadja vagy adatbázisból betölti a cache-bejegyzést.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="sessionId">Az elérni kívánt játékosmunkamenet azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CacheReadResult> GetOrLoadLockedAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default);

        /// <summary>
        /// A játékoslock alatt ellenőrzi a gyorsítótárban tárolt munkamenetet.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="sessionId">Az ellenőrzendő munkamenet-azonosító.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CacheReadStatus> CheckSessionLockedAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default);

        /// <summary>
        /// A játékoslock alatt érvényesíti az új munkamenetet.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="sessionId">Az érvényesítendő új munkamenet-azonosító.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<bool> NewSessionCheckLockedAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default);

        /// <summary>
        /// A játékos gyorsítótárazott állapotát a saját lockja alatt módosítja.
        /// A callback null értékkel elutasíthatja a műveletet, siker esetén pedig
        /// visszaadja a mentendő játékos-adatszegmenseket.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="update">A játékoslock alatt végrehajtott módosítás; <see langword="null"/> eredménnyel elutasíthatja a mentést.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CacheUpdateResult> UpdatePlayerLockedAsync(
            int playerId,
            string sessionId,
            Func<CachedPlayer, DirtyFlags?> update,
            CancellationToken ct = default);

        /// <summary>
        /// A játékos- és kérdéscache aktuális állapotát ugyanazon játékoslock
        /// alatt adja át, miközben a módosítás játékos dirty jelzőt állít.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="update">A játékoslock alatt végrehajtott módosítás; <see langword="null"/> eredménnyel elutasíthatja a mentést.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CacheUpdateResult> UpdatePlayerAndQuestionsLockedAsync(
            int playerId,
            string sessionId,
            Func<CachedPlayer, CachedQuestion, DirtyFlags?> update,
            CancellationToken ct = default);

        /// <summary>
        /// A szerver által már hitelesített meccseredményt az aktuális
        /// klienssessiontől függetlenül vezeti át a játékoscache-en.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="loadSessionId">A meccs indításakor rögzített munkamenet-azonosító.</param>
        /// <param name="update">A játékoslock alatt végrehajtott módosítás; <see langword="null"/> eredménnyel elutasíthatja a mentést.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<bool> UpdateRewardPlayerLockedAsync(
            int playerId,
            string loadSessionId,
            Func<CachedPlayer, DirtyFlags?> update,
            CancellationToken ct = default);

        /// <summary>
        /// A kérdéscache-t a játékos saját lockja alatt módosítja.
        /// A callback null értékkel elutasíthatja a műveletet, siker esetén pedig
        /// visszaadja a mentendő kérdésslotok bitmaskját.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="update">A játékoslock alatt végrehajtott módosítás; <see langword="null"/> eredménnyel elutasíthatja a mentést.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CacheUpdateResult> UpdateQuestionsLockedAsync(
            int playerId,
            string sessionId,
            Func<CachedPlayer, CachedQuestion, uint?> update,
            CancellationToken ct = default);

        /// <summary>
        /// A szerver által már hitelesített meccs sajátkérdés-
        /// statisztikáját az aktuális klienssessiontől függetlenül menti.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="loadSessionId">A meccs indításakor rögzített munkamenet-azonosító.</param>
        /// <param name="update">A játékoslock alatt végrehajtott módosítás; <see langword="null"/> eredménnyel elutasíthatja a mentést.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<bool> UpdateRewardQuestionsLockedAsync(
            int playerId,
            string loadSessionId,
            Func<CachedPlayer, CachedQuestion, uint?> update,
            CancellationToken ct = default);

        /// <summary>
        /// A játékoslock alatt kijelentkezésre jelöli a munkamenetet.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="sessionId">A lezárandó munkamenet-azonosító.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CacheUpdateResult> LogoutLockedRequestAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default);
        /// <summary>
        /// A játékoslock alatt tartós tárba írja a módosított játékosadatokat.
        /// </summary>
        Task<(SaveResult, bool)> SaveDirtyLockedAsync(int playerId, CancellationToken ct = default);

        /// <summary>
        /// A játékoslock alatt tartós tárba írja a módosított kérdésadatokat.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<int> SaveDirtyQuestionLockedAsync(int playerId, CancellationToken ct = default);

    }
}
