namespace KvizCommando.Server.Application.Abstractions.Security
{
    /// <summary>
    /// Az e-mail-címek egységesítését és visszafejtés nélkül kereshető, kulcsos kivonatát biztosítja.
    /// </summary>
    public interface IEmailLookup
    {
        /// <summary>
        /// Egységes keresési formára alakítja az e-mail-címet.
        /// </summary>
        /// <param name="email">A normalizálandó e-mail-cím.</param>
        /// <returns>Az összehasonlításhoz és kivonatképzéshez normalizált e-mail-cím.</returns>
        string Normalize(string email);

        /// <summary>
        /// Kiszámítja egy már normalizált e-mail-cím kulcsos keresési kivonatát.
        /// </summary>
        /// <param name="normalizedEmail">A szolgáltatás szabályai szerint már normalizált e-mail-cím.</param>
        /// <returns>A normalizált e-mail-cím kulcsos keresési kivonata.</returns>
        byte[] ComputeNormalizedHash(string normalizedEmail);

        /// <summary>
        /// Normalizálja az e-mail-címet, majd kiszámítja a kulcsos keresési kivonatát.
        /// </summary>
        /// <param name="email">A feldolgozandó e-mail-cím.</param>
        /// <returns>A normalizált e-mail-cím kulcsos keresési kivonata.</returns>
        byte[] ComputeHashFromRaw(string email);
    }
}
