using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace KvizCommando.Server.Startup;

public static class AuthenticationDiagnosticsExtensions
{
    /// <summary>
    /// Bekapcsolja a cookie- és bearer-hitelesítés személyes adatot nem tartalmazó diagnosztikáját, ha a konfiguráció engedélyezi.
    /// </summary>
    /// <param name="app">A konfigurálandó webalkalmazás.</param>
    /// <param name="configuration">A diagnosztikai kapcsolót tartalmazó konfiguráció.</param>
    /// <returns>A további middleware-ekhez használható webalkalmazás.</returns>
    public static WebApplication UseAuthenticationDiagnostics(
        this WebApplication app,
        IConfiguration configuration)
    {
        if (!app.Environment.IsDevelopment() ||
            !configuration.GetValue<bool>(
                "Diagnostics:EnableAuthenticationDebugLogging"))
        {
            return app;
        }

        app.MapGet("/signin-facebook", async context =>
        {
            var result = await context.AuthenticateAsync(
                IdentityConstants.ExternalScheme);

            app.Logger.LogInformation(
                "External authentication callback succeeded={Succeeded}",
                result.Succeeded);
        });

        app.Use(async (context, next) =>
        {
            var cookieAuthentication = await context.AuthenticateAsync(
                IdentityConstants.ApplicationScheme);
            var bearerAuthentication = await context.AuthenticateAsync(
                IdentityConstants.BearerScheme);
            var hasBearerHeader = context.Request.Headers["Authorization"]
                .ToString()
                .StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

            app.Logger.LogInformation(
                "AUTH path={Path} cookie={CookieAuthenticated} bearerHeader={HasBearerHeader} bearer={BearerAuthenticated}",
                context.Request.Path,
                cookieAuthentication.Succeeded,
                hasBearerHeader,
                bearerAuthentication.Succeeded);

            await next();

            app.Logger.LogInformation(
                "AUTH response status={StatusCode} path={Path} endpoint={Endpoint}",
                context.Response.StatusCode,
                context.Request.Path,
                context.GetEndpoint()?.DisplayName ?? "<none>");
        });

        return app;
    }
}
