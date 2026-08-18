using KvizCommando.Server.Background;
using KvizCommando.Server.Services.PlayerCache;

namespace KvizCommando.Server.Startup;

public static class KvizCommandoBackgroundWorkerExtensions
{
    /// <summary>
    /// Regisztrálja a lejárt tokenek és a játékos-cache tartósításának háttérfolyamatait.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddKvizCommandoBackgroundWorkers(
        this IServiceCollection services)
    {
        services.AddHostedService<ExpiredTokenKillerService>();
        services.AddSingleton<GameDbFlushService>();
        services.AddHostedService<PlayerCachePersistenceService>();

        return services;
    }
}
