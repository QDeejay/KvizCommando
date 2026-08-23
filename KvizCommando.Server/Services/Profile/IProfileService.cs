using KvizCommando.Shared.Contracts.Profile;

namespace KvizCommando.Server.Services.Profile;

public interface IProfileService
{
    /// <summary>Betölti a munkamenethez tartozó csapat- és avatarprofilt.</summary>
    /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
    /// <param name="sessionId">Az aktív játékmenet azonosítója.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A profil és a lekérés állapota.</returns>
    Task<ProfileLoadResponse> GetAsync(
        int playerId,
        string sessionId,
        CancellationToken ct = default);

    /// <summary>Ellenőrzi, hogy a megadott csapatnév menthető-e.</summary>
    /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
    /// <param name="request">Az ellenőrizendő csapatnév és a munkamenet adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A normalizált név és az ellenőrzés eredménye.</returns>
    Task<CheckTeamNameResponse> CheckTeamNameAsync(
        int playerId,
        CheckTeamNameRequest request,
        CancellationToken ct = default);

    /// <summary>Elmenti a játékos új csapatnevét.</summary>
    /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
    /// <param name="request">Az új csapatnév és a munkamenet adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A frissített profil és a mentés állapota.</returns>
    Task<SaveProfileResponse> SaveTeamNameAsync(
        int playerId,
        SaveTeamNameRequest request,
        CancellationToken ct = default);

    /// <summary>Elmenti a játékos új kapitányavatarját.</summary>
    /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
    /// <param name="request">Az új avatar és a munkamenet adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A frissített profil és a mentés állapota.</returns>
    Task<SaveProfileResponse> SaveAvatarAsync(
        int playerId,
        SaveAvatarRequest request,
        CancellationToken ct = default);
}
