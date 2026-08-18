using KvizCommando.Server.Authorization;
using KvizCommando.Server.Data;
using KvizCommando.Server.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.Facebook;

namespace KvizCommando.Server.Identity;

public static class IdentityServiceCollectionExtensions
{
    /// <summary>
    /// Regisztrálja és konfigurálja az alkalmazás Identity szolgáltatásait.
    /// </summary>
    public static IServiceCollection AddCustomIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                IdentityConfiguration.ConfigureIdentityOptions(options);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders()
            .AddApiEndpoints();
        // A böngészős felület cookie-t, a mobil és asztali kliens opaque bearer tokent használhat.
        services.AddCustomAuthentication(configuration);

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Api", p =>
                p.AddAuthenticationSchemes(
                     IdentityConstants.ApplicationScheme,   // cookie
                     IdentityConstants.BearerScheme)
                 .RequireAuthenticatedUser());

            options.AddPolicy(TermsAcceptedRequirement.PolicyName, p =>
                p.AddAuthenticationSchemes(
                       IdentityConstants.ApplicationScheme,
                       IdentityConstants.BearerScheme
                        )
       .RequireAuthenticatedUser()
       .AddRequirements(new TermsAcceptedRequirement()));
        });

        services.Configure<BearerTokenOptions>(IdentityConstants.BearerScheme, options =>
        {
            options.BearerTokenExpiration = TimeSpan.FromMinutes(15);
            options.RefreshTokenExpiration = TimeSpan.FromDays(7);
        });

        // A beállítás jelenleg csak a későbbi GDPR-folyamat bővítési pontja.
        // A hiányzó export- és törlési lépéseket a docs/infrastructure-status.md rögzíti.
        services.Configure<PersonalDataOptions>(options =>
        {
            options.ProtectionKeyName = null;
        });

        return services;
    }
    public class PersonalDataOptions
    {
        public string? ProtectionKeyName { get; set; }
    }
}
