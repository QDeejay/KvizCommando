namespace KvizCommando.Server.Application.Abstractions.Security
{
    /// <summary>
    /// Adatmező-szintű, hitelesített titkosítás szerződése.
    /// </summary>
    public interface IEncryptionProvider
    {
        /// <summary>
        /// Titkosítja a megadott szöveget, és visszaadja az AES-GCM tárolási részeit.
        /// </summary>
        /// <param name="plain">A titkosítandó szöveg.</param>
        /// <param name="context">A rekordhoz és mezőhöz kötő, nem titkos hitelesítési kontextus.</param>
        /// <returns>A titkosított tartalom, az egyszeri nonce és a hitelesítési címke.</returns>
        (byte[] Cipher, byte[] Nonce, byte[] Tag) Encrypt(string plain, string context);

        /// <summary>
        /// Visszafejti és hitelesíti az AES-GCM-mel tárolt értéket.
        /// </summary>
        /// <param name="cipher">A visszafejtendő titkosított bájtsorozat.</param>
        /// <param name="nonce">A titkosításhoz tartozó egyszer használatos érték.</param>
        /// <param name="tag">A titkosított adat hitelesítési címkéje.</param>
        /// <param name="context">A titkosításkor használt rekord- és mezőkontextus.</param>
        /// <returns>A visszafejtett eredeti szöveg.</returns>
        string Decrypt(byte[] cipher, byte[] nonce, byte[] tag, string context);
    }
}
