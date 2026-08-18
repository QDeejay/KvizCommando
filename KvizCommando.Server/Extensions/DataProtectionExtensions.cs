using Microsoft.AspNetCore.DataProtection;

namespace KvizCommando.Server.Extensions;

public static class DataProtectionExtensions
{
    /// <summary>
    /// Regisztrálja és konfigurálja az alkalmazás adatvédelmi kulcskezelését.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <param name="cfg">Az alkalmazás konfigurációja.</param>
    /// <param name="env">Az aktuális futtatási környezet adatai.</param>
    public static IServiceCollection AddAppDataProtection(this IServiceCollection services, IConfiguration cfg, IHostEnvironment env)
    {
        var dp = services.AddDataProtection();
        var keyRingPath = cfg["DataProtection:KeyRingPath"];
        if (!string.IsNullOrWhiteSpace(keyRingPath))
        {
            Directory.CreateDirectory(keyRingPath);
            dp.PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));
        }

        var applicationName = cfg["DataProtection:ApplicationName"] ?? env.ApplicationName;
        dp.SetApplicationName(applicationName);
        return services;
    }
}
