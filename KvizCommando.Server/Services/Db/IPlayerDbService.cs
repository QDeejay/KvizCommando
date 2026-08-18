using KvizCommando.Server.Identity;
using KvizCommando.Server.Services.PlayerCache;

namespace KvizCommando.Server.Services.Db
{
    public interface IPlayerDbService
    {
        /// <summary>
        /// Betölti a játékos teljes, gyorsítótárba helyezhető állapotát az adatbázisból.
        /// </summary>
        Task<CachedPlayer?> LoadPlayerFromDbAsync(
            int playerId,
            string sessionId,
            CancellationToken ct);
        /// <summary>
        /// A dirty jelzők alapján elmenti a játékos módosított adatszegmenseit.
        /// </summary>
        Task<bool> SavePlayerToDbAsync(
            CachedPlayer player,
            DirtyFlags flags,
            int playerId,
            CancellationToken ct);
        /// <summary>
        /// Betölti a beléptetéshez szükséges Identity-, játékosnév- és játékosazonosító-adatokat.
        /// </summary>
        Task<(ApplicationUser?, string?, int?)> LoadCheckinDataFromDbAsync(
            string userId,
            CancellationToken ct);
        /// <summary>
        /// Ellenőrzi és elmenti a felhasználó nyilvános játékosnevét.
        /// </summary>
        Task<(IReadOnlyList<string>, bool success)> SaveDisplayNameToDbAsync(
            ApplicationUser user,
            string displayName,
            CancellationToken ct);

        /// <summary>
        /// Append-only auditbejegyzésként elmenti az ÁSZF elfogadását.
        /// </summary>
        Task<bool> SaveTermsToDbAsync(
             ApplicationUser user,
             string acceptedTerms,
             string currentTerms,
             DateTime acceptedAt,
             CancellationToken ct);

        /// <summary>
        /// Létrehozza az Identity-felhasználóhoz tartozó játékosrekordot és alapadatait.
        /// </summary>
        Task<int> CreatePlayerToDbAsync(
            string userId,
            string displayname,
            string teamname,
            CancellationToken ct);
        /// <summary>
        /// Szabad játékosnevet javasol a külső szolgáltatótól kapott név alapján.
        /// </summary>
        Task<string> SuggestAsync(
            string? rawName,
            CancellationToken ct = default);
    }
}
