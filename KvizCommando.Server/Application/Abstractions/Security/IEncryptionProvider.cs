namespace KvizCommando.Server.Application.Abstractions.Security
{
    /// <summary>
    /// Adatmező-szintű, hitelesített titkosítás szerződése.
    /// A jelenlegi fejlesztési implementáció nem biztosít valódi titkosítást.
    /// Élesben: AES-256-GCM, Key Vault kulcsmenedzsmenttel.
    /// </summary>
    public interface IEncryptionProvider
    {
        (byte[] Cipher, byte[] Nonce, byte[] Tag) Encrypt(string plain);
        /// <summary>
        /// Visszaalakítja a fejlesztési kódolással tárolt értéket.
        /// </summary>
        /// <param name="cipher">A visszafejtendő titkosított bájtsorozat.</param>
        /// <param name="nonce">A titkosításhoz tartozó egyszer használatos érték.</param>
        /// <param name="tag">A titkosított adat hitelesítési címkéje.</param>
        /// <returns>A visszafejtett eredeti szöveg.</returns>
        string Decrypt(byte[] cipher, byte[] nonce, byte[] tag);
    }
}
