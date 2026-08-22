namespace KvizCommando.Server.Services.Profile;

public interface IProfileAccountDeletionService
{
    /// <summary>Jelszavas újrahitelesítés után véglegesen törli a felhasználói fiókot és a hozzá tartozó adatokat.</summary>
    /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
    /// <param name="currentPassword">A fiók jelenlegi jelszava.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns>A törlési művelet eredménye.</returns>
    Task<ProfileAccountDeletionServiceState> DeleteAsync(
        string userId,
        string currentPassword,
        CancellationToken ct = default);
}

public enum ProfileAccountDeletionServiceState
{
    Success,
    InvalidPassword,
    NotFound,
    ServerError
}
