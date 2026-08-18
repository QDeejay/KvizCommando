using System.Globalization;

namespace KvizCommando.Server.Extensions;

public static class LocalizationExtensions
{
    /// <summary>
    /// Regisztrálja a szerveroldali lokalizáció szolgáltatásait.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    public static IServiceCollection AddAppLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options => { options.ResourcesPath = "Resources"; });
        return services;
    }

    /// <summary>
    /// Bekapcsolja a kérésenkénti lokalizációt a támogatott kultúrákkal.
    /// </summary>
    /// <param name="app">A konfigurálandó alkalmazás vagy végpontépítő.</param>
    /// <param name="defaultCulture">Az alapértelmezett kultúra neve.</param>
    /// <param name="supportedCultures">Az alkalmazás által támogatott kultúrák nevei.</param>
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
