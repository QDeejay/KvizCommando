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
        ///
        /// --- Dual scheme: Cookie + Bearer ---
        /// 1) AUTHENTICATION: Cookie (UI) + OPAQUE Bearer (desktop/mobil)
        /// 
        services.AddCustomAuthentication(configuration);

        
        ///
        /// Autorizáció sémák
        /// 
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Api", p =>
                p.AddAuthenticationSchemes(
                     IdentityConstants.ApplicationScheme,   // cookie
                     IdentityConstants.BearerScheme
                     // opaque bearer
                        )
                 .RequireAuthenticatedUser());

            options.AddPolicy(TermsAcceptedRequirement.PolicyName, p =>
                p.AddAuthenticationSchemes(
                       IdentityConstants.ApplicationScheme,
                       IdentityConstants.BearerScheme
                        )
       .RequireAuthenticatedUser()
       .AddRequirements(new TermsAcceptedRequirement()));
        });

        ///
        /// ===== Bearer token beállítások =====
        /// 
        services.Configure<BearerTokenOptions>(IdentityConstants.BearerScheme, options =>
        {
            options.BearerTokenExpiration = TimeSpan.FromMinutes(15);
            options.RefreshTokenExpiration = TimeSpan.FromDays(7);
        });

        // ===== Personal data (GDPR export/törlés) =====
        services.Configure<PersonalDataOptions>(options =>
        {
            options.ProtectionKeyName = null; // vagy saját provider
        });

        return services;
    }
    public class PersonalDataOptions
    {
        public string? ProtectionKeyName { get; set; }
    }
}
