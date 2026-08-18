namespace KvizCommando.Server.Services.Security
{
    public interface ISessionService
    {
        /// <summary>
        /// Új munkamenetkulcsot hoz létre és társít a felhasználóhoz.
        /// </summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <returns>Az újonnan létrehozott munkamenetkulcs.</returns>
        string GenerateAndStoreSessionKey(string userId);

        /// <summary>
        /// Visszakeresi a felhasználóhoz tartozó aktív munkamenetkulcsot.
        /// </summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <returns>A tárolt munkamenetkulcs, vagy <see langword="null"/>, ha nincs aktív bejegyzés.</returns>
        string? GetSessionKey(string userId);
    }
}
