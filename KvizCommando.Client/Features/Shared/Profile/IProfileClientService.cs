using KvizCommando.Shared.Contracts.CheckIn;
using KvizCommando.Shared.Contracts.Profile;

namespace KvizCommando.Client.Features.Shared.Profile;

public interface IProfileClientService
{
    /// <summary>Betölti a csapat- és avatarprofilt.</summary>
    Task<ProfileLoadResponse> GetAsync(CancellationToken ct = default);

    /// <summary>Ellenőrzi, hogy a megadott csapatnév menthető-e.</summary>
    /// <param name="teamName">Az ellenőrizendő csapatnév.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A normalizált név és az ellenőrzés eredménye.</returns>
    Task<CheckTeamNameResponse> CheckTeamNameAsync(
        string teamName,
        CancellationToken ct = default);

    /// <summary>Elmenti a bejelentkezett játékos csapatnevét.</summary>
    /// <param name="teamName">Az új csapatnév.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A frissített profil és a mentés állapota.</returns>
    Task<SaveProfileResponse> SaveTeamNameAsync(
        string teamName,
        CancellationToken ct = default);

    /// <summary>Elmenti a bejelentkezett játékos kapitányavatarját.</summary>
    /// <param name="captainAvatar">Az új avatar száma szöveges formában.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A frissített profil és a mentés állapota.</returns>
    Task<SaveProfileResponse> SaveAvatarAsync(
        string captainAvatar,
        CancellationToken ct = default);

    /// <summary>Betölti a bejelentkezett felhasználó fiókadatait.</summary>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A fiókadatok és a lekérés állapota.</returns>
    Task<ProfileAccountResponse> GetAccountAsync(CancellationToken ct = default);

    /// <summary>Elmenti a titkosított kapcsolattartási és számlázási adatokat.</summary>
    /// <param name="request">A mentendő kapcsolattartási és számlázási adatok.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A mentett fiókadatok és a művelet állapota.</returns>
    Task<ProfileAccountResponse> SaveAccountAsync(SaveProfileAccountRequest request, CancellationToken ct = default);

    /// <summary>Az aktuális kliensnyelvre frissíti a kommunikációs nyelvet.</summary>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A frissített fiókadatok és a művelet állapota.</returns>
    Task<ProfileAccountResponse> UpdatePreferredLocaleAsync(CancellationToken ct = default);

    /// <summary>Elindítja az Identity e-mail-csere folyamatát.</summary>
    /// <param name="newEmail">A megerősítésre váró új e-mail-cím.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A művelet sikeressége és az Identity hibái.</returns>
    Task<ProfileIdentityUpdateResponse> RequestEmailChangeAsync(string newEmail, CancellationToken ct = default);

    /// <summary>Az Identity segítségével módosítja az aktuális jelszót.</summary>
    /// <param name="currentPassword">A jelenlegi jelszó.</param>
    /// <param name="newPassword">Az új jelszó.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A művelet sikeressége és az Identity hibái.</returns>
    Task<ProfileIdentityUpdateResponse> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken ct = default);

    /// <summary>Betölti az aktuális, kultúrafüggő jogi dokumentum metaadatait.</summary>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>Az aktuális jogi dokumentum metaadatai, vagy <see langword="null"/>, ha a kérés sikertelen.</returns>
    Task<TermsMeta?> GetLegalMetaAsync(CancellationToken ct = default);

    /// <summary>Jelszavas újrahitelesítéssel lekéri a személyesadat-export ZIP-fájlját.</summary>
    /// <param name="currentPassword">A fiók jelenlegi jelszava.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>Az export állapota és siker esetén a letölthető fájl.</returns>
    Task<ProfileDataExportResult> ExportDataAsync(
        string currentPassword,
        CancellationToken ct = default);
}
