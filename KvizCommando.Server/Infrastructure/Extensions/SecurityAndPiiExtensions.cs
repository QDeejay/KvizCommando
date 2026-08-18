using KvizCommando.Server.Application.Abstractions.Security;
using KvizCommando.Server.Application.Security;
using KvizCommando.Server.Infrastructure.Options;

namespace KvizCommando.Server.Infrastructure.Extensions;

public static class SecurityAndPiiExtensions
{
    /// <summary>
    /// Regisztrálja a személyes adatok keresési, titkosítási és tárolási absztrakcióit.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <param name="configuration">A biztonsági beállításokat tartalmazó konfiguráció.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddSecurityAndPii(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SecurityOptions>(
            configuration.GetSection("Security"));

        services.AddSingleton<IEmailLookup, EmailLookup>();
        services.AddSingleton<IEncryptionProvider, DummyEncryptionProvider>();
        services.AddScoped<IUserPiiService, DummyUserPiiService>();

        return services;
    }
}
