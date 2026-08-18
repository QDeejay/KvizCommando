using KvizCommando.Client.Http;

namespace KvizCommando.Client.DependencyInjection;

public static class ClientHttpPipelineExtensions
{
    private const string DEFAULT_CLIENT_NAME = "DefaultWithLang";

    /// <summary>
    /// Regisztrálja az alapértelmezett API-klienst és a kérések állapot-, nyelv-, hitelesítési és értesítési feldolgozóit.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <param name="baseAddress">A szerver végpontjainak alapcíme.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddClientHttpPipeline(
        this IServiceCollection services,
        string baseAddress)
    {
        services.AddTransient<LoaderHandler>();
        services.AddTransient<LanguageHandler>();
        services.AddTransient<AuthRedirectHandler>();
        services.AddTransient<ToastHandler>();
        services.AddTransient<LoggingHandler>();

        services.AddScoped(serviceProvider =>
        {
            var factory = serviceProvider
                .GetRequiredService<IHttpClientFactory>();

            return factory.CreateClient(DEFAULT_CLIENT_NAME);
        });

        services
            .AddHttpClient(DEFAULT_CLIENT_NAME, client =>
            {
                client.BaseAddress = new Uri(baseAddress);
            })
            .AddHttpMessageHandler<LoaderHandler>()
            .AddHttpMessageHandler<LanguageHandler>()
            .AddHttpMessageHandler<AuthRedirectHandler>()
            .AddHttpMessageHandler<ToastHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        return services;
    }
}
