using System;
using System.Text;
using KvizCommando.Server.Application.Abstractions.Security;

namespace KvizCommando.Server.Application.Security
{
    /// <summary>
    /// Fejlesztési helyettesítő, amely Base64-kódolással tölti ki a titkosított adat mezőit.
    /// Nem nyújt bizalmasságot, ezért valós személyes adat kezelésére nem használható.
    /// Az éles megoldás követelményeit a docs/infrastructure-status.md tartalmazza.
    /// </summary>
    public class DummyEncryptionProvider : IEncryptionProvider
    {
        private static readonly byte[] DummyNonce = new byte[12];
        private static readonly byte[] DummyTag = new byte[16];

        /// <summary>
        /// A fejlesztési tárolási formára alakítja a megadott szöveget.
        /// </summary>
        public (byte[] Cipher, byte[] Nonce, byte[] Tag) Encrypt(string plain)
        {
            var cipher = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(plain)));
            return (cipher, DummyNonce, DummyTag);
        }

        /// <inheritdoc />
        public string Decrypt(byte[] cipher, byte[] nonce, byte[] tag)
        {
            var b64 = Encoding.UTF8.GetString(cipher);
            var plainBytes = Convert.FromBase64String(b64);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
