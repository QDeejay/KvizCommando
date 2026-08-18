namespace KvizCommando.Server.Services.PlayerCache
{
    public interface IPlayerCacheService
    {

        IReadOnlyCollection<int> GetActivePlayerIds();
        Task<CacheReadResult> GetOrLoadLockedAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default);

        Task<CacheReadStatus> CheckSessionLockedAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default);

        Task<bool> NewSessionCheckLockedAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default);

        /// <summary>
        /// A játékos cache-elt állapotát a saját lockja alatt módosítja.
        /// A callback null értékkel elutasíthatja a műveletet, siker esetén pedig
        /// visszaadja a mentendő player-szegmenseket.
        /// </summary>
        Task<CacheUpdateResult> UpdatePlayerLockedAsync(
            int playerId,
            string sessionId,
            Func<CachedPlayer, DirtyFlags?> update,
            CancellationToken ct = default);

        /// <summary>
        /// A játékos- és kérdéscache aktuális állapotát ugyanazon játékoslock
        /// alatt adja át, miközben a módosítás player dirty flaget állít.
        /// </summary>
        Task<CacheUpdateResult> UpdatePlayerAndQuestionsLockedAsync(
            int playerId,
            string sessionId,
            Func<CachedPlayer, CachedQuestion, DirtyFlags?> update,
            CancellationToken ct = default);

        /// <summary>
        /// A szerver által már hitelesített meccseredményt az aktuális
        /// klienssessiontől függetlenül vezeti át a játékoscache-en.
        /// </summary>
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
        Task<CacheUpdateResult> UpdateQuestionsLockedAsync(
            int playerId,
            string sessionId,
            Func<CachedPlayer, CachedQuestion, uint?> update,
            CancellationToken ct = default);

        /// <summary>
        /// A szerver által már hitelesített meccs sajátkérdés-
        /// statisztikáját az aktuális klienssessiontől függetlenül menti.
        /// </summary>
        Task<bool> UpdateRewardQuestionsLockedAsync(
            int playerId,
            string loadSessionId,
            Func<CachedPlayer, CachedQuestion, uint?> update,
            CancellationToken ct = default);

        Task<CacheUpdateResult> LogoutLockedRequestAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default);
        Task<(SaveResult, bool)> SaveDirtyLockedAsync(int playerId, CancellationToken ct = default);

        Task<int> SaveDirtyQuestionLockedAsync(int playerId, CancellationToken ct = default);

    }
}
