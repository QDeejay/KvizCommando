namespace KvizCommando.Server.Infrastructure.Options;

/// <summary>
/// A mezőszintű PII-titkosítás konfigurációját tartalmazza.
/// </summary>
public sealed class PiiEncryptionOptions
{
    /// <summary>A konfigurációs szakasz neve.</summary>
    public const string SECTION_NAME = "PiiEncryption";

    /// <summary>Az AES-256 kulcs kötelező hossza bájtban.</summary>
    public const int KEY_SIZE_BYTES = 32;

    /// <summary>A Base64-formátumban megadott, 32 bájtos AES-256 kulcs.</summary>
    public string Key { get; set; } = string.Empty;
}
