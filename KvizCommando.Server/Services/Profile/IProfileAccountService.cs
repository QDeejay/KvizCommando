using KvizCommando.Shared.Contracts.Profile;

namespace KvizCommando.Server.Services.Profile;

public interface IProfileAccountService
{
    /// <summary>Betölti az Identity e-mailt és a titkosított PII-adatokat.</summary>
    /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A fiókadatok és a lekérés állapota.</returns>
    Task<ProfileAccountResponse> GetAsync(string userId, CancellationToken ct = default);

    /// <summary>Atomikusan menti a profil kapcsolattartási és számlázási adatait.</summary>
    /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
    /// <param name="request">A mentendő kapcsolattartási és számlázási adatok.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A mentett fiókadatok és a művelet állapota.</returns>
    Task<ProfileAccountResponse> SaveAsync(string userId, SaveProfileAccountRequest request, CancellationToken ct = default);

    /// <summary>Az aktuális klienskultúrára frissíti a felhasználó kommunikációs nyelvét.</summary>
    /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
    /// <param name="preferredLocale">A mentendő támogatott kultúra.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A frissített fiókadatok és a művelet állapota.</returns>
    Task<ProfileAccountResponse> UpdatePreferredLocaleAsync(
        string userId,
        string preferredLocale,
        CancellationToken ct = default);
}
