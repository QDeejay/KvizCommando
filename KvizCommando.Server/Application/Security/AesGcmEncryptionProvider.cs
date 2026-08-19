using System.Security.Cryptography;
using System.Text;
using KvizCommando.Server.Application.Abstractions.Security;
using KvizCommando.Server.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace KvizCommando.Server.Application.Security;

/// <summary>
/// AES-256-GCM használatával titkosítja és hitelesíti a személyesadat-mezőket.
/// </summary>
public sealed class AesGcmEncryptionProvider : IEncryptionProvider
{
    private const int NONCE_SIZE_BYTES = 12;
    private const int TAG_SIZE_BYTES = 16;

    private readonly byte[] _key;

    /// <summary>
    /// Létrehozza a titkosítási szolgáltatást a konfigurált AES-256 kulccsal.
    /// </summary>
    /// <param name="options">A PII-titkosítás ellenőrzött konfigurációja.</param>
    public AesGcmEncryptionProvider(IOptions<PiiEncryptionOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _key = Convert.FromBase64String(options.Value.Key);
        if (_key.Length != PiiEncryptionOptions.KEY_SIZE_BYTES)
        {
            throw new InvalidOperationException(
                "A PiiEncryption:Key beállításnak 32 bájtos AES-256 kulcsot kell tartalmaznia.");
        }
    }

    /// <inheritdoc />
    public (byte[] Cipher, byte[] Nonce, byte[] Tag) Encrypt(string plain, string context)
    {
        ArgumentNullException.ThrowIfNull(plain);
        ArgumentException.ThrowIfNullOrWhiteSpace(context);

        var plainBytes = Encoding.UTF8.GetBytes(plain);
        var contextBytes = Encoding.UTF8.GetBytes(context);
        var cipher = new byte[plainBytes.Length];
        var nonce = RandomNumberGenerator.GetBytes(NONCE_SIZE_BYTES);
        var tag = new byte[TAG_SIZE_BYTES];

        using var aes = new AesGcm(_key, TAG_SIZE_BYTES);
        aes.Encrypt(nonce, plainBytes, cipher, tag, contextBytes);

        return (cipher, nonce, tag);
    }

    /// <inheritdoc />
    public string Decrypt(byte[] cipher, byte[] nonce, byte[] tag, string context)
    {
        ArgumentNullException.ThrowIfNull(cipher);
        ArgumentNullException.ThrowIfNull(nonce);
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentException.ThrowIfNullOrWhiteSpace(context);

        if (nonce.Length != NONCE_SIZE_BYTES || tag.Length != TAG_SIZE_BYTES)
        {
            throw new CryptographicException(
                "A titkosított személyes adat nonce- vagy tagmérete érvénytelen.");
        }

        var plainBytes = new byte[cipher.Length];
        var contextBytes = Encoding.UTF8.GetBytes(context);

        using var aes = new AesGcm(_key, TAG_SIZE_BYTES);
        aes.Decrypt(nonce, cipher, tag, plainBytes, contextBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
