using KvizCommando.Server.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace KvizCommando.Server.DependencyInjection;

public static class KvizCommandoWebExtensions
{
    /// <summary>
    /// Regisztrálja a webes végpontok, a SignalR, a lokalizáció és az API-dokumentáció keretrendszer-szolgáltatásait.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddKvizCommandoWeb(
        this IServiceCollection services)
    {
        services.AddControllersWithViews();
        services.AddRazorPages();
        services.AddMemoryCache();
        services.AddSignalR();
        services.AddAppProblemDetails();
        services.AddAppLocalization();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "KvizCommando API",
                    Version = "v1"
                });

            options.AddSecurityDefinition(
                "oauth2",
                new OpenApiSecurityScheme
                {
                    Description = "Authorization header using the Bearer scheme (\"bearer {token}\")",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });

        return services;
    }
}
