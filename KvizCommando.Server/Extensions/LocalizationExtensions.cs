using System.Globalization;

namespace KvizCommando.Server.Extensions;

public static class LocalizationExtensions
{
    public static IServiceCollection AddAppLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options => { options.ResourcesPath = "Resources"; });
        return services;
    }

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
