using KvizCommando.Server.Extensions;
using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Email;
using KvizCommando.Server.Infrastructure.Extensions;
using KvizCommando.Server.Infrastructure.Logging;
using KvizCommando.Server.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;

namespace KvizCommando.Server.DependencyInjection;

public static class KvizCommandoSecurityExtensions
{
    /// <summary>
    /// Regisztrálja az adatvédelmi, PII-, audit-, e-mail- és kérésvédelmi infrastruktúrát.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <param name="configuration">A biztonsági és hálózati beállításokat tartalmazó konfiguráció.</param>
    /// <param name="environment">Az adatvédelmi kulcstér nevéhez használt futtatási környezet.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddKvizCommandoSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSecurityAndPii(configuration);
        services.AddAppCors(configuration);
        services.AddAppRateLimiting();
        services.AddAppDataProtection(configuration, environment);

        services.Configure<AppOptions>(
            configuration.GetSection("App"));
        services.AddTransient<IEmailSender<ApplicationUser>, WhitelistedEmailSender>();
        services.AddScoped<ICallbackUrlValidator, CallbackUrlValidator>();
        services.AddScoped<IAuditLogger, AuditLogger>();

        return services;
    }
}
