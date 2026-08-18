namespace KvizCommando.Server.Application.Abstractions.Security
{
    /// <summary>
    /// Adatmező-szintű titkosítás absztrakció. Dummy: pass-through/„fake GCM”.
    /// Élesben: AES-256-GCM, Key Vault kulcsmenedzsmenttel.
    /// </summary>
    public interface IEncryptionProvider
    {
        (byte[] Cipher, byte[] Nonce, byte[] Tag) Encrypt(string plain);
        string Decrypt(byte[] cipher, byte[] nonce, byte[] tag);
    }
}
