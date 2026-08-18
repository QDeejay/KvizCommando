namespace KvizCommando.Server.Services.Security
{
    public interface ISessionService
    {
        /// <summary>
        /// Új munkamenetkulcsot hoz létre és társít a felhasználóhoz.
        /// </summary>
        string GenerateAndStoreSessionKey(string userId);
        string? GetSessionKey(string userId);
    }
}
