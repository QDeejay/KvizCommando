using System.Globalization;

namespace KvizCommando.Server.Startup;

public static class LocalizationPipelineExtensions
{
    /// <summary>
    /// Bekapcsolja a kérésenkénti lokalizációt a támogatott kultúrákkal.
    /// </summary>
    /// <param name="app">A konfigurálandó alkalmazás vagy végpontépítő.</param>
    /// <param name="defaultCulture">Az alapértelmezett kultúra neve.</param>
    /// <param name="supportedCultures">Az alkalmazás által támogatott kultúrák nevei.</param>
    public static IApplicationBuilder UseAppLocalization(
        this IApplicationBuilder app,
        string defaultCulture,
        string[] supportedCultures)
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
