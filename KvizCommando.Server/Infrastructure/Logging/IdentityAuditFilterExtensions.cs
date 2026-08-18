
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;


namespace KvizCommando.Server.Infrastructure.Logging;

public static class IdentityAuditFilterExtensions
{
    /// <summary>
    /// Auditnaplózást kapcsol az Identity végpontokra.
    /// </summary>
    /// <param name="builder">A kiegészítendő végpontkonvenció-építő.</param>
    public static IEndpointConventionBuilder WithIdentityAudit(this IEndpointConventionBuilder builder)
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            var httpContext = context.HttpContext;
            var audit = httpContext.RequestServices.GetRequiredService<IAuditLogger>();

            var path = httpContext.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            var userId = httpContext.User?.Identity?.IsAuthenticated == true
                ? httpContext.User.FindFirst("sub")?.Value
                : null;
            var ip = httpContext.Connection.RemoteIpAddress?.ToString();

            // Elfelejtett jelszó
            if (path.EndsWith("/forgotpassword"))
            {
                await audit.LogAsync(AuditEvents.ForgotPasswordRequested, "Anonymous", ip);

                var result = await next(context);

                if (result is IResult r && r is not null && r.GetType().Name.Contains("Problem"))
                {
                    await audit.LogAsync(AuditEvents.ForgotPasswordEmailNotFound, "Anonymous", ip);
                }
                else
                {
                    var resolvedUserId = httpContext.Items["ResolvedUserId"] as string ?? "UnknownUser";
                    await audit.LogAsync(AuditEvents.ForgotPasswordEmailSent, resolvedUserId, ip);
                }

                return result;
            }

            // Jelszó-visszaállítás
            if (path.EndsWith("/resetpassword"))
            {
                await audit.LogAsync(AuditEvents.PasswordResetAttempted, "Anonymous", ip);

                var result = await next(context);

                if (result is IResult r && r is not null && r.GetType().Name.Contains("Problem"))
                {
                    var resolvedUserId = httpContext.Items["ResolvedUserId"] as string ?? "UnknownUser";
                    await audit.LogAsync(AuditEvents.PasswordResetFailed, resolvedUserId, ip);
                }
                else
                {
                    var resolvedUserId = httpContext.Items["ResolvedUserId"] as string ?? "UnknownUser";
                    await audit.LogAsync(AuditEvents.PasswordResetSucceeded, resolvedUserId, ip);
                }

                return result;
            }

            // Hitelesített profil- és jelszómódosítás
            if (path.EndsWith("/manage/info") && httpContext.Request.Method == HttpMethods.Post)
            {
                await audit.LogAsync(AuditEvents.ManageInfoRequested, userId ?? "Anonymous", ip);

                var result = await next(context);

                if (result is IResult r && r is not null && r.GetType().Name.Contains("Problem"))
                {
                    await audit.LogAsync(AuditEvents.ManageInfoPasswordChangeFailed, userId ?? "Anonymous", ip);
                }
                else
                {
                    await audit.LogAsync(AuditEvents.ManageInfoPasswordChangeSucceeded, userId ?? "Anonymous", ip);
                }

                return result;
            }

            return await next(context);
        });

        return builder;
    }
}
