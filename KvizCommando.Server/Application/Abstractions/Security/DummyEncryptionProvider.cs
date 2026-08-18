using System;
using System.Text;
using KvizCommando.Server.Application.Abstractions.Security;

namespace KvizCommando.Server.Application.Security
{
    /// <summary>
    /// DUMMY: nem valódi titkosítás. Csak Base64 + fix Nonce/Tag, hogy a mezők kitöltve legyenek.
    /// Éles váltáskor ezt cseréled AES-GCM implementációra.
    /// </summary>
    public class DummyEncryptionProvider : IEncryptionProvider
    {
        private static readonly byte[] DummyNonce = new byte[12]; // 12 byte
        private static readonly byte[] DummyTag = new byte[16];   // 16 byte

        public (byte[] Cipher, byte[] Nonce, byte[] Tag) Encrypt(string plain)
        {
            var cipher = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(plain)));
            return (cipher, DummyNonce, DummyTag);
        }

        public string Decrypt(byte[] cipher, byte[] nonce, byte[] tag)
        {
            var b64 = Encoding.UTF8.GetString(cipher);
            var plainBytes = Convert.FromBase64String(b64);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
