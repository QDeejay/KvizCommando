using System.Globalization;

namespace KvizCommando.Server.Extensions;

public static class LocalizationExtensions
{
    /// <summary>
    /// Regisztrálja a szerveroldali lokalizáció szolgáltatásait.
    /// </summary>
    public static IServiceCollection AddAppLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options => { options.ResourcesPath = "Resources"; });
        return services;
    }

    /// <summary>
    /// Bekapcsolja a kérésenkénti lokalizációt a támogatott kultúrákkal.
    /// </summary>
    public static IApplicationBuilder UseAppLocalization(this IApplicationBuilder app, string defaultCulture, string[] supportedCultures)
    {
        var options = new RequestLocalizationOptions()
            .SetDefaultCulture(defaultCulture)
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);

        app.UseRequestLocalization(options);

        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(defaultCulture);
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(defaultCulture);

        return app;
    }
}
