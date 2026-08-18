namespace KvizCommando.Server.Infrastructure.Options
{
    /// <summary>
    /// Biztonsági beállítások. Pepper és AE kapcsolók. Töltsd fel appsettings-ből.
    /// </summary>
    public class SecurityOptions
    {
        /// <summary>A normalizált e-mail-hash alkalmazásszintű titkos kiegészítője.</summary>
        public string EmailHashPepper { get; set; } = "";

        /// <summary>Az SQL Server Always Encrypted támogatásának kapcsolója.</summary>
        public bool EnableAlwaysEncrypted { get; set; } = false;
    }
}
