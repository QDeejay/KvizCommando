namespace KvizCommando.Server.Infrastructure.Options
{
    /// <summary>
    /// Biztonsági beállítások. Pepper és AE kapcsolók. Töltsd fel appsettings-ből.
    /// </summary>
    public class SecurityOptions
    {
        /// <summary>Pepper a normált e-mail hash-hez. (Key Vault-ban lesz élesben.)</summary>
        public string EmailHashPepper { get; set; } = "";

        /// <summary>Always Encrypted használata SQL Serveren (Column Encryption Setting).</summary>
        public bool EnableAlwaysEncrypted { get; set; } = false;
    }
}
