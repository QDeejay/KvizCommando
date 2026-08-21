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
        var benefitSecret = configuration[$"{RegistrationBenefitHashOptions.SECTION_NAME}:Secret"];
        if (!string.IsNullOrWhiteSpace(benefitSecret) &&
            (string.Equals(benefitSecret, configuration["AuditHash:Secret"], StringComparison.Ordinal) ||
             string.Equals(benefitSecret, configuration[$"{PiiEncryptionOptions.SECTION_NAME}:Key"], StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                "A RegistrationBenefitHash:Secret nem lehet azonos az AuditHash vagy a PiiEncryption titkával.");
        }

        services.AddOptions<PiiEncryptionOptions>()
            .Bind(configuration.GetSection(PiiEncryptionOptions.SECTION_NAME))
            .Validate(
                options => HasValidKey(options.Key),
                "A PiiEncryption:Key beállításnak 32 bájtos, Base64-formátumú AES-kulcsot kell tartalmaznia.")
            .ValidateOnStart();

        services.AddOptions<RegistrationBenefitHashOptions>()
            .Bind(configuration.GetSection(RegistrationBenefitHashOptions.SECTION_NAME))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Secret) && options.Secret.Length >= 32,
                "A RegistrationBenefitHash:Secret legalább 32 karakteres, külön titok legyen.")
            .ValidateOnStart();

        services.AddSingleton<IEncryptionProvider, AesGcmEncryptionProvider>();
        services.AddScoped<IUserPiiService, UserPiiService>();
        services.AddScoped<IRegistrationBenefitClaimService, RegistrationBenefitClaimService>();

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
