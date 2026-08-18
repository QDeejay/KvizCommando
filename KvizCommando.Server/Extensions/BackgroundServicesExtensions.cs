using KvizCommando.Server.Background;
using KvizCommando.Server.Services.PlayerCache;

namespace KvizCommando.Server.Extensions;

public static class BackgroundServicesExtensions
{
    public static IServiceCollection AddBackgroundWorkers(this IServiceCollection services)
    {
        services.AddHostedService<ExpiredTokenKillerService>();
        services.AddSingleton<GameDbFlushService>();
        services.AddHostedService<PlayerCachePersistenceService>();
        // később ide jöhet majd:
        // services.AddHostedService<InactiveUserNotifierService>();

        return services;
    }
}
