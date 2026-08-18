using KvizCommando.Server.Authorization;
using KvizCommando.Server.Identity;
using KvizCommando.Server.Services.Auth;
using KvizCommando.Server.Services.CheckIn;
using KvizCommando.Server.Services.Security;
using Microsoft.AspNetCore.Authorization;

namespace KvizCommando.Server.Startup;

public static class KvizCommandoIdentityExtensions
{
    /// <summary>
    /// Regisztrálja a hitelesítést, a jogosultsági szabályokat, a beléptetést és a munkamenet-kezelést.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <param name="configuration">A hitelesítési szolgáltatók konfigurációja.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddKvizCommandoIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddCustomIdentity(configuration);

        services.AddSingleton<ISessionService, SessionService>();
        services.AddScoped<ITermsProvider, TermsProvider>();
        services.AddScoped<ICheckInService, CheckInService>();
        services.AddScoped<IClaimsSyncService, ClaimsSyncService>();
        services.AddScoped<IAuthorizationHandler, TermsAcceptedHandler>();

        return services;
    }
}
