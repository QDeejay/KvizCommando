using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;

namespace KvizCommando.Server.Identity;

public static class AuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// Regisztrálja a cookie- és bearer-alapú hitelesítési sémákat.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <param name="configuration">Az alkalmazás konfigurációja.</param>
    public static IServiceCollection AddCustomAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthentication(options =>
            {
                // A böngészős kérések alapértelmezett sémája az alkalmazás cookie-ja.
                options.DefaultScheme = IdentityConstants.ApplicationScheme;

                // A külső szolgáltató válaszát külön, rövid életű cookie kezeli.
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ExternalScheme, options =>
            {
                options.Cookie.Name = "QC_External_CookieCooker";
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .AddCookie(IdentityConstants.TwoFactorRememberMeScheme)
            .AddCookie(IdentityConstants.TwoFactorUserIdScheme)
            .AddBearerToken(IdentityConstants.BearerScheme)
            .AddFacebook(options =>
            {

                options.AppId = configuration["Authentication:Facebook:AppId"];
                options.AppSecret = configuration["Authentication:Facebook:AppSecret"];
                options.CallbackPath = "/signin-facebook";
                options.Scope.Add("email");
                options.SaveTokens = true;

                options.Events = new OAuthEvents
                {
                    OnRemoteFailure = ctx =>
                    {
                        var redirect = ctx.Properties?.RedirectUri;
                        if (string.IsNullOrEmpty(redirect))
                            redirect = "/";

                        var err = ctx.Request.Query["error"].ToString();
                        var reas = ctx.Request.Query["error_reason"].ToString();
                        var desc = ctx.Request.Query["error_description"].ToString();

                        // Az egységes hibakódot a kliens szolgáltatófüggetlenül tudja feldolgozni.
                        var code = string.IsNullOrEmpty(err) ? "external_login_failed" : err;

                        var sep = redirect.Contains('?') ? '&' : '?';
                        var q = $"{sep}error={Uri.EscapeDataString(code)}";
                        if (!string.IsNullOrEmpty(reas)) q += $"&reason={Uri.EscapeDataString(reas)}";
                        if (!string.IsNullOrEmpty(desc)) q += $"&desc={Uri.EscapeDataString(desc)}";

                        ctx.Response.Redirect(redirect + q);
                        ctx.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            });

        // A bélyeg minden kérésnél történő ellenőrzése azonnal érvényteleníti a visszavont munkameneteket.
        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.Zero;
        });

        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(24);
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "Quiz_Commando_CookieCooker";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;

            options.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal =
                        SecurityStampValidator.ValidatePrincipalAsync,

                OnRedirectToLogin = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api"))
                    {
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                    ctx.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api"))
                    {
                        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                    ctx.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
