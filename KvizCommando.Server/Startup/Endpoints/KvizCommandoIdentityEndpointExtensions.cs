using KvizCommando.Server.Identity;
using Microsoft.AspNetCore.Identity;

namespace KvizCommando.Server.Startup;

public static class KvizCommandoIdentityEndpointExtensions
{
    /// <summary>
    /// Leképezi az alkalmazás Identity-, kijelentkezési és külső hitelesítési végpontjait.
    /// </summary>
    /// <param name="endpoints">A végpontokat fogadó útvonalépítő.</param>
    /// <param name="configuration">Az Identity-végpontok konfigurációja.</param>
    /// <returns>A további végpontokhoz használható útvonalépítő.</returns>
    public static IEndpointRouteBuilder MapKvizCommandoIdentityEndpoints(
        this IEndpointRouteBuilder endpoints,
        IConfiguration configuration)
    {
        endpoints.MapGroup("/")
            .MapIdentityApi<ApplicationUser>()
            .WithPerEndpointRateLimiting()
            .WithIdentityAudit()
            .WithPublicAuthAvailability(configuration);

        endpoints.MapLogoutEndpoints();
        endpoints.MapFacebookAuthEndpoints(
            IdentityConfiguration.IsFacebookLoginEnabled(configuration));

        return endpoints;
    }
}
