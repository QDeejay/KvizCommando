using KvizCommando.Server.Application.Abstractions.Security;
using KvizCommando.Server.Application.Security;
using KvizCommando.Server.Infrastructure.Options;

namespace KvizCommando.Server.Startup;

public static class SecurityAndPiiExtensions
{
    /// <summary>
    /// Regisztrálja a személyes adatok keresési, titkosítási és tárolási absztrakcióit.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <param name="configuration">A biztonsági beállításokat tartalmazó konfiguráció.</param>
    /// <param name="environment">Az adapterek engedélyezését meghatározó futtatási környezet.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddSecurityAndPii(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<SecurityOptions>(
            configuration.GetSection("Security"));

        services.AddSingleton<IEmailLookup, EmailLookup>();

        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "A személyesadat-mezők production titkosítási adaptere még nincs konfigurálva. " +
                "A DummyEncryptionProvider kizárólag Development környezetben használható.");
        }

        services.AddSingleton<IEncryptionProvider, DummyEncryptionProvider>();
        services.AddScoped<IUserPiiService, UserPiiService>();

        return services;
    }
}
