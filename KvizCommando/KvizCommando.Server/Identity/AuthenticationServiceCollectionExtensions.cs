using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;

namespace KvizCommando.Server.Identity;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddCustomAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthentication(options =>
            {
                // alap séma → Identity.Application
                options.DefaultScheme = IdentityConstants.ApplicationScheme;

                // ideiglenes külső login cookie → Identity.External
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            // végleges auth cookie (WASM, web)
            .AddCookie(IdentityConstants.ApplicationScheme)
            // ideiglenes külső provider cookie
            .AddCookie(IdentityConstants.ExternalScheme, options =>
            {
                options.Cookie.Name = "QC_External_CookieCooker";
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            // opaque bearer token (mobil/desktop)
            .AddBearerToken(IdentityConstants.BearerScheme)
            // Facebook provider
            .AddFacebook(options =>
            {

                options.AppId = configuration["Authentication:Facebook:AppId"];
                options.AppSecret = configuration["Authentication:Facebook:AppSecret"];
                options.CallbackPath = "/signin-facebook"; // gyári endpoint
                options.Scope.Add("email");           
                options.SaveTokens = true; // <<< fontos

                // extra log, hogy lásd a raw payloadot
                options.Events = new OAuthEvents
                {
                    OnRemoteFailure = ctx =>
                    {
                        // Facebook visszadobta: error=access_denied, error_reason=user_denied, stb.
                        var redirect = ctx.Properties?.RedirectUri;
                        if (string.IsNullOrEmpty(redirect))
                            redirect = "/"; // ha nincs beállítva, essünk vissza a főoldalra

                        // Gyűjtsük ki a FB által adott hibákat
                        var err = ctx.Request.Query["error"].ToString();              // pl. access_denied
                        var reas = ctx.Request.Query["error_reason"].ToString();       // pl. user_denied
                        var desc = ctx.Request.Query["error_description"].ToString();  // pl. Permissions error

                        // Állítsunk be egy egységes kódot – ha nincs, legyen 'external_login_failed'
                        var code = string.IsNullOrEmpty(err) ? "external_login_failed" : err;

                        // Építsük fel az egységes ?error=... query-t
                        var sep = redirect.Contains('?') ? '&' : '?';
                        var q = $"{sep}error={Uri.EscapeDataString(code)}";
                        if (!string.IsNullOrEmpty(reas)) q += $"&reason={Uri.EscapeDataString(reas)}";
                        if (!string.IsNullOrEmpty(desc)) q += $"&desc={Uri.EscapeDataString(desc)}";

                        ctx.Response.Redirect(redirect + q);
                        ctx.HandleResponse(); // fontos: ne dobja tovább az exception-t
                        return Task.CompletedTask;
                    },

                    OnCreatingTicket = ctx =>
                    {
                        foreach (var kv in ctx.Properties.Items)
                        {
                            Console.WriteLine($"[OnCreatingTicket] {kv.Key} = {kv.Value}");
                        }
                        return Task.CompletedTask;
                    }
                   
                };
                // Plusz log az auth URL-hez
               
            });

        // SecurityStamp validator
        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.Zero;
        });

        // Data protection token providers
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(24);
        });

        // Application cookie finomhangolás
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "Quiz_Commando_CookieCooker";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.SlidingExpiration = true;

            options.Events = new CookieAuthenticationEvents
            {
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







/*
 * using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Identity;

namespace KvizCommando.Server.Identity;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddCustomAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;

                options.DefaultSignInScheme = IdentityConstants.ExternalScheme; 
            })
            .AddCookie(IdentityConstants.ApplicationScheme) // WASM browser
            .AddCookie(IdentityConstants.ExternalScheme)
            .AddBearerToken(IdentityConstants.BearerScheme) // Mobile/desktop
            .AddFacebook(options =>
            {
                options.AppId = configuration["Authentication:Facebook:AppId"];
                options.AppSecret = configuration["Authentication:Facebook:AppSecret"];
                options.CallbackPath = "/signin-facebook";
            });

        // ===== SecurityStamp validator =====
        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.Zero;
        });

        // ===== Data protection token providers =====
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(24);
        });

        // ===== Application cookie =====
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "Quiz_Commando_CookieCooker";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.SlidingExpiration = true;

            options.Events = new CookieAuthenticationEvents
            {
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
        services.ConfigureExternalCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            
        });
        return services;
    }
}
 */