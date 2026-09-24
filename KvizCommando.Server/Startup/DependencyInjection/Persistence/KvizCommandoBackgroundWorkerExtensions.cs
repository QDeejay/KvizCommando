using KvizCommando.Server.Background;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.Rankings;

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
        services.AddOptions<PlayerCachePersistenceOptions>()
            .BindConfiguration(PlayerCachePersistenceOptions.SECTION_NAME)
            .Validate(options => options.FlushIntervalSeconds >= 5 &&
                                 options.FlushIntervalSeconds <= 3600,
                "FlushIntervalSeconds must be between 5 and 3600.")
            .ValidateOnStart();
        services.AddHostedService<ExpiredTokenKillerService>();
        services.AddSingleton<GameDbFlushService>();
        services.AddSingleton<IRankingCacheService, RankingCacheService>();
        services.AddHostedService<PlayerCachePersistenceService>();

        return services;
    }
}
