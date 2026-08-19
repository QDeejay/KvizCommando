using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Email;
using KvizCommando.Server.Infrastructure.Logging;
using KvizCommando.Server.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;

namespace KvizCommando.Server.Startup;

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
        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SECTION_NAME))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.OutputRoot),
                "Az Email:OutputRoot beállítás kötelező.")
            .Validate(
                options => !options.OutputRoot.Contains(
                    "wwwroot",
                    StringComparison.OrdinalIgnoreCase),
                "Az e-mail-fájlok könyvtára nem lehet a nyilvános wwwroot alatt.")
            .Validate(
                options => options.BaseUrls.TryGetValue(
                    options.ActiveBaseUrl,
                    out var value) &&
                    Uri.TryCreate(value, UriKind.Absolute, out _),
                "Az Email:ActiveBaseUrl beállításhoz érvényes abszolút URL szükséges.")
            .ValidateOnStart();

        var emailService = configuration[$"{EmailOptions.SECTION_NAME}:Service"];
        if (string.Equals(
                emailService,
                EmailOptions.FILE_SERVICE,
                StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IEmailDelivery, FileEmailDelivery>();
        }
        else if (string.Equals(
                     emailService,
                     EmailOptions.MAIL_SERVICE,
                     StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Az Email:Service értéke Mail, de valódi levélküldő adapter még nincs regisztrálva.");
        }
        else
        {
            throw new InvalidOperationException(
                $"Ismeretlen Email:Service érték: '{emailService}'. Használható érték: File vagy Mail.");
        }

        services.AddTransient<IEmailSender<ApplicationUser>, WhitelistedEmailSender>();
        services.Configure<CallbackWhitelistOptions>(options =>
        {
            var baseUrls = configuration
                .GetSection($"{EmailOptions.SECTION_NAME}:BaseUrls")
                .Get<Dictionary<string, string>>() ?? new();

            options.AllowedDomains = baseUrls.Values
                .Select(value => Uri.TryCreate(
                    value,
                    UriKind.Absolute,
                    out var uri)
                        ? uri.Host
                        : null)
                .Where(host => !string.IsNullOrWhiteSpace(host))
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        });
        services.AddSingleton<ICallbackUrlValidator, CallbackUrlValidator>();

        services.AddOptions<AuditOptions>()
            .Bind(configuration.GetSection(AuditOptions.SECTION_NAME))
            .Validate(
                options => string.Equals(
                    options.Provider,
                    "File",
                    StringComparison.OrdinalIgnoreCase),
                "Jelenleg csak az Audit:Provider = File adapter érhető el.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.OutputRoot),
                "Az Audit:OutputRoot beállítás kötelező.")
            .Validate(
                options => !options.OutputRoot.Contains(
                    "wwwroot",
                    StringComparison.OrdinalIgnoreCase),
                "Az auditkönyvtár nem lehet a nyilvános wwwroot alatt.")
            .Validate(
                options => options.RetentionDays is >= 1 and <= 3650,
                "Az Audit:RetentionDays értékének 1 és 3650 nap közé kell esnie.")
            .ValidateOnStart();
        services.AddSingleton<IAuditLogger, FileAuditLogger>();

        return services;
    }
}
