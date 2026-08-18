using KvizCommando.Server.Background;
using KvizCommando.Server.Services.PlayerCache;

namespace KvizCommando.Server.Extensions;

public static class BackgroundServicesExtensions
{
    /// <summary>
    /// Regisztrálja az alkalmazás háttérfolyamatait.
    /// </summary>
    public static IServiceCollection AddBackgroundWorkers(this IServiceCollection services)
    {
        services.AddHostedService<ExpiredTokenKillerService>();
        services.AddSingleton<GameDbFlushService>();
        services.AddHostedService<PlayerCachePersistenceService>();

        return services;
    }
}
