using KvizCommando.Server.Application.Abstractions.Security;
using KvizCommando.Server.Application.Security;
using KvizCommando.Server.Infrastructure.Options;

namespace KvizCommando.Server.Startup;

/// <summary>
/// A mezőszintű PII-titkosítás és -tárolás szolgáltatásregisztrációit tartalmazza.
/// </summary>
public static class SecurityAndPiiExtensions
{
    /// <summary>
    /// Regisztrálja a személyes adatok titkosítási és tárolási szolgáltatásait.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <param name="configuration">A biztonsági beállításokat tartalmazó konfiguráció.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddSecurityAndPii(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<PiiEncryptionOptions>()
            .Bind(configuration.GetSection(PiiEncryptionOptions.SECTION_NAME))
            .Validate(
                options => HasValidKey(options.Key),
                "A PiiEncryption:Key beállításnak 32 bájtos, Base64-formátumú AES-kulcsot kell tartalmaznia.")
            .ValidateOnStart();

        services.AddSingleton<IEncryptionProvider, AesGcmEncryptionProvider>();
        services.AddScoped<IUserPiiService, UserPiiService>();

        return services;
    }

    private static bool HasValidKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        try
        {
            return Convert.FromBase64String(key).Length == PiiEncryptionOptions.KEY_SIZE_BYTES;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
