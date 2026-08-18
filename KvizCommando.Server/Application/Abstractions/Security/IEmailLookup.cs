namespace KvizCommando.Server.Application.Abstractions.Security
{
    /// <summary>
    /// E-mail normalizálás + hash (pepperrel). Singleton.
    /// </summary>
    public interface IEmailLookup
    {
        /// <summary>
        /// Egységes keresési formára alakítja az e-mail-címet.
        /// </summary>
        string Normalize(string email);
        byte[] ComputeNormalizedHash(string normalizedEmail);
        byte[] ComputeHashFromRaw(string email); // convenience
    }
}
