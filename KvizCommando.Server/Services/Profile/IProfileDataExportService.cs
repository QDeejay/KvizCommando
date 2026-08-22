namespace KvizCommando.Server.Services.Profile;

public interface IProfileDataExportService
{
    /// <summary>Elkészíti a hitelesített felhasználó lokalizált személyesadat-exportját.</summary>
    /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
    /// <param name="currentPassword">A fiók jelenlegi jelszava.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>Az export állapota és siker esetén a letölthető ZIP tartalma.</returns>
    Task<ProfileDataExportServiceResult> ExportAsync(
        string userId,
        string currentPassword,
        CancellationToken ct = default);
}

public enum ProfileDataExportServiceState
{
    Success,
    InvalidPassword,
    NotFound,
    ServerError
}

public sealed class ProfileDataExportServiceResult
{
    public ProfileDataExportServiceState State { get; set; }
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = [];
}
