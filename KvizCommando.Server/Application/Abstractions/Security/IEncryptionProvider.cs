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
        string Decrypt(byte[] cipher, byte[] nonce, byte[] tag);
    }
}
