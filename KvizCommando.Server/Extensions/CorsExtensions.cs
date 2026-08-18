namespace KvizCommando.Server.Extensions;

public static class CorsExtensions
{
    /// <summary>
    /// Regisztrálja az alkalmazás CORS-szabályait.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <param name="cfg">Az alkalmazás konfigurációja.</param>
    public static IServiceCollection AddAppCors(this IServiceCollection services, IConfiguration cfg)
    {
        var allowedOrigins = cfg.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        services.AddCors(options =>
        {
            options.AddPolicy("Spa", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });
        return services;
    }
}
