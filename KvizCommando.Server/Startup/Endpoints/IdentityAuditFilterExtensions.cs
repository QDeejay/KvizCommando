using KvizCommando.Server.Infrastructure.Logging;
using System.Security.Claims;

namespace KvizCommando.Server.Startup;

public static class IdentityAuditFilterExtensions
{
    /// <summary>
    /// A biztonsági szempontból releváns Identity-műveletekhez auditbejegyzést kapcsol.
    /// </summary>
    /// <param name="builder">A kiegészítendő Identity-végpontcsoport.</param>
    /// <returns>A további konvenciókhoz használható végpontépítő.</returns>
    public static IEndpointConventionBuilder WithIdentityAudit(
        this IEndpointConventionBuilder builder)
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            var httpContext = context.HttpContext;
            var path = httpContext.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            if (path.EndsWith("/forgotpassword"))
            {
                return await ExecuteAuditedAsync(
                    context,
                    next,
                    AuditEvents.ForgotPassword,
                    AuditOutcome.Accepted,
                    subjectId: null);
            }

            if (path.EndsWith("/resetpassword"))
            {
                return await ExecuteAuditedAsync(
                    context,
                    next,
                    AuditEvents.PasswordReset,
                    AuditOutcome.Succeeded,
                    subjectId: null);
            }

            if (path.EndsWith("/manage/info") &&
                httpContext.Request.Method == HttpMethods.Post)
            {
                var userId = httpContext.User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                return await ExecuteAuditedAsync(
                    context,
                    next,
                    AuditEvents.ManageInfo,
                    AuditOutcome.Succeeded,
                    userId);
            }

            return await next(context);
        });

        return builder;
    }

    private static async ValueTask<object?> ExecuteAuditedAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next,
        string eventName,
        AuditOutcome successfulOutcome,
        string? subjectId)
    {
        var httpContext = context.HttpContext;
        var audit = httpContext.RequestServices.GetRequiredService<IAuditLogger>();

        try
        {
            var result = await next(context);
            var outcome = IsFailedResult(result)
                ? AuditOutcome.Failed
                : successfulOutcome;

            await WriteAuditAsync(
                audit,
                httpContext,
                eventName,
                outcome,
                subjectId);
            return result;
        }
        catch
        {
            await WriteAuditAsync(
                audit,
                httpContext,
                eventName,
                AuditOutcome.Failed,
                subjectId);
            throw;
        }
    }

    private static Task WriteAuditAsync(
        IAuditLogger audit,
        HttpContext httpContext,
        string eventName,
        AuditOutcome outcome,
        string? subjectId)
    {
        return audit.LogAsync(new AuditEntry(
            eventName,
            outcome,
            subjectId,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.TraceIdentifier));
    }

    private static bool IsFailedResult(object? result) =>
        result is IStatusCodeHttpResult { StatusCode: >= 400 };
}
