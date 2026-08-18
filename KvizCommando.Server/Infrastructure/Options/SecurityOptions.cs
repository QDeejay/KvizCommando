namespace KvizCommando.Server.Infrastructure.Options
{
    /// <summary>
    /// Az e-mail-keresési kivonat és az SQL Server oszloptitkosítás konfigurációs értékeit tartalmazza.
    /// </summary>
    public class SecurityOptions
    {
        /// <summary>A normalizált e-mail-hash alkalmazásszintű titkos kiegészítője.</summary>
        public string EmailHashPepper { get; set; } = "";

        /// <summary>Az SQL Server Always Encrypted támogatásának kapcsolója.</summary>
        public bool EnableAlwaysEncrypted { get; set; } = false;
    }
}
