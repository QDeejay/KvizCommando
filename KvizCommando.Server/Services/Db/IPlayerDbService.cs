using KvizCommando.Server.Identity;
using KvizCommando.Server.Services.PlayerCache;

namespace KvizCommando.Server.Services.Db
{
    public interface IPlayerDbService
    {
        /// <summary>
        /// Betölti a játékos teljes, gyorsítótárba helyezhető állapotát az adatbázisból.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CachedPlayer?> LoadPlayerFromDbAsync(
            int playerId,
            string sessionId,
            CancellationToken ct);
        /// <summary>
        /// A dirty jelzők alapján elmenti a játékos módosított adatszegmenseit.
        /// </summary>
        /// <param name="player">A mentendő gyorsítótárazott játékosállapot.</param>
        /// <param name="flags">A módosult játékos-adatszegmenseket jelölő bitmező.</param>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
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
        /// <param name="user">Az érintett Identity-felhasználó.</param>
        /// <param name="acceptedTerms">A felhasználó által elfogadott feltételverzió.</param>
        /// <param name="currentTerms">Az elfogadáskor aktuális feltételverzió.</param>
        /// <param name="acceptedAt">Az elfogadás UTC időpontja.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<bool> SaveTermsToDbAsync(
             ApplicationUser user,
             string acceptedTerms,
             string currentTerms,
             DateTime acceptedAt,
             CancellationToken ct);

        /// <summary>
        /// Létrehozza az Identity-felhasználóhoz tartozó játékosrekordot és alapadatait.
        /// </summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <param name="displayname">A létrehozandó játékos nyilvános neve.</param>
        /// <param name="teamname">A létrehozandó játékos alapértelmezett csapatneve.</param>
        /// <param name="startingCredit">A játékos induló kreditegyenlege.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<int> CreatePlayerToDbAsync(
            string userId,
            string displayname,
            string teamname,
            int startingCredit,
            CancellationToken ct);
        /// <summary>
        /// Ellenőrzi, hogy a normalizált csapatnév más játékos tartós adatai között szerepel-e.
        /// </summary>
        Task<bool> IsNormalizedTeamNameTakenAsync(
            string normalizedTeamName,
            int excludedPlayerId,
            CancellationToken ct = default);
        /// <summary>
        /// Szabad játékosnevet javasol a külső szolgáltatótól kapott név alapján.
        /// </summary>
        /// <param name="rawName">A névjavaslat kiinduló értéke, amely hiányozhat.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<string> SuggestAsync(
            string? rawName,
            CancellationToken ct = default);
    }
}
