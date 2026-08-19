namespace KvizCommando.Server.Startup;

public static class LocalizationServiceCollectionExtensions
{
    /// <summary>
    /// Regisztrálja a szerveroldali lokalizáció szolgáltatásait és erőforráskönyvtárát.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddAppLocalization(
        this IServiceCollection services)
    {
        services.AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        });

        return services;
    }
}
