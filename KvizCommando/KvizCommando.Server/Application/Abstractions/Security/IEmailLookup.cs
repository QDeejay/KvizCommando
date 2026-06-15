namespace KvizCommando.Server.Application.Abstractions.Security
{
    /// <summary>
    /// E-mail normalizálás + hash (pepperrel). Singleton.
    /// </summary>
    public interface IEmailLookup
    {
        string Normalize(string email);
        byte[] ComputeNormalizedHash(string normalizedEmail);
        byte[] ComputeHashFromRaw(string email); // convenience
    }
}
