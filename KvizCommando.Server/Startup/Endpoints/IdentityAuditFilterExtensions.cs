using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
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
            var path = context.HttpContext.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            if (path.EndsWith("/register"))
            {
                return await AuditRegistrationAsync(context, next);
            }

            if (path.EndsWith("/login"))
            {
                return await AuditLoginAsync(context, next);
            }

            if (path.EndsWith("/forgotpassword"))
            {
                return await AuditPasswordResetRequestAsync(context, next);
            }

            if (path.EndsWith("/resetpassword"))
            {
                return await AuditPasswordResetAsync(context, next);
            }

            if (path.EndsWith("/manage/info") &&
                context.HttpContext.Request.Method == HttpMethods.Post)
            {
                return await AuditManageInfoAsync(context, next);
            }

            if (path.EndsWith("/confirmemail") &&
                context.HttpContext.Request.Query.ContainsKey("changedEmail"))
            {
                return await AuditEmailChangeAsync(context, next);
            }

            return await next(context);
        });

        return builder;
    }

    private static async ValueTask<object?> AuditRegistrationAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var request = context.Arguments.OfType<RegisterRequest>().FirstOrDefault();
        var audit = httpContext.RequestServices.GetRequiredService<IAuditLogger>();

        try
        {
            var result = await next(context);
            var failed = IsFailedResult(result);
            var subjectId = failed
                ? null
                : await FindUserIdByEmailAsync(httpContext, request?.Email);

            await WriteAuditAsync(
                audit,
                httpContext,
                AuditEvents.AccountRegistered,
                failed ? AuditOutcome.Failed : AuditOutcome.Succeeded,
                failed ? null : subjectId,
                subjectId);
            return result;
        }
        catch
        {
            var subjectId = await FindUserIdByEmailAsync(
                httpContext,
                request?.Email);
            await WriteAuditAsync(
                audit,
                httpContext,
                AuditEvents.AccountRegistered,
                subjectId is null
                    ? AuditOutcome.Failed
                    : AuditOutcome.Succeeded,
                actorId: subjectId,
                subjectId: subjectId);
            throw;
        }
    }

    private static async ValueTask<object?> AuditLoginAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var request = context.Arguments.OfType<LoginRequest>().FirstOrDefault();
        var audit = httpContext.RequestServices.GetRequiredService<IAuditLogger>();

        try
        {
            var result = await next(context);
            var userManager = httpContext.RequestServices
                .GetRequiredService<UserManager<ApplicationUser>>();
            var user = string.IsNullOrWhiteSpace(request?.Email)
                ? null
                : await userManager.FindByEmailAsync(request.Email);
            var subjectId = user?.Id;

            if (!IsFailedResult(result))
            {
                await WriteAuditAsync(
                    audit,
                    httpContext,
                    AuditEvents.Login,
                    AuditOutcome.Succeeded,
                    subjectId,
                    subjectId);
            }
            else
            {
                var locked = user is not null &&
                             await userManager.IsLockedOutAsync(user);
                await WriteAuditAsync(
                    audit,
                    httpContext,
                    locked ? AuditEvents.AccountLocked : AuditEvents.Login,
                    locked ? AuditOutcome.Denied : AuditOutcome.Failed,
                    actorId: null,
                    subjectId: subjectId);
            }

            return result;
        }
        catch
        {
            await WriteAuditAsync(
                audit,
                httpContext,
                AuditEvents.Login,
                AuditOutcome.Failed,
                actorId: null,
                subjectId: null);
            throw;
        }
    }

    private static async ValueTask<object?> AuditPasswordResetRequestAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var audit = httpContext.RequestServices.GetRequiredService<IAuditLogger>();

        try
        {
            var result = await next(context);
            await WriteAuditAsync(
                audit,
                httpContext,
                AuditEvents.PasswordResetRequested,
                IsFailedResult(result) ? AuditOutcome.Failed : AuditOutcome.Accepted,
                actorId: null,
                subjectId: null);
            return result;
        }
        catch
        {
            await WriteAuditAsync(
                audit,
                httpContext,
                AuditEvents.PasswordResetRequested,
                AuditOutcome.Failed,
                actorId: null,
                subjectId: null);
            throw;
        }
    }

    private static async ValueTask<object?> AuditPasswordResetAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var request = context.Arguments.OfType<ResetPasswordRequest>().FirstOrDefault();
        var audit = httpContext.RequestServices.GetRequiredService<IAuditLogger>();
        var subjectId = await FindUserIdByEmailAsync(httpContext, request?.Email);

        try
        {
            var result = await next(context);
            var failed = IsFailedResult(result);
            await WriteAuditAsync(
                audit,
                httpContext,
                AuditEvents.PasswordReset,
                failed ? AuditOutcome.Failed : AuditOutcome.Succeeded,
                failed ? null : subjectId,
                subjectId);
            return result;
        }
        catch
        {
            await WriteAuditAsync(
                audit,
                httpContext,
                AuditEvents.PasswordReset,
                AuditOutcome.Failed,
                actorId: null,
                subjectId: subjectId);
            throw;
        }
    }

    private static async ValueTask<object?> AuditManageInfoAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<InfoRequest>().FirstOrDefault();
        if (string.IsNullOrWhiteSpace(request?.NewPassword))
        {
            return await next(context);
        }

        var httpContext = context.HttpContext;
        var audit = httpContext.RequestServices.GetRequiredService<IAuditLogger>();
        var subjectId = GetAuthenticatedUserId(httpContext);
        var details = new AuditDetails(ChangedFields: ["Password"]);

        try
        {
            var result = await next(context);
            await WriteAuditAsync(
                audit,
                httpContext,
                AuditEvents.PasswordChanged,
                IsFailedResult(result) ? AuditOutcome.Failed : AuditOutcome.Succeeded,
                subjectId,
                subjectId,
                details);
            return result;
        }
        catch
        {
            await WriteAuditAsync(
                audit,
                httpContext,
                AuditEvents.PasswordChanged,
                AuditOutcome.Failed,
                subjectId,
                subjectId,
                details);
            throw;
        }
    }

    private static async ValueTask<object?> AuditEmailChangeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var audit = httpContext.RequestServices.GetRequiredService<IAuditLogger>();
        var subjectId = httpContext.Request.Query["userId"].ToString();
        var details = new AuditDetails(ChangedFields: ["Email"]);

        try
        {
            var result = await next(context);
            var failed = IsFailedResult(result);
            await WriteAuditAsync(
                audit,
                httpContext,
                AuditEvents.EmailChanged,
                failed ? AuditOutcome.Failed : AuditOutcome.Succeeded,
                failed ? null : subjectId,
                string.IsNullOrWhiteSpace(subjectId) ? null : subjectId,
                details);
            return result;
        }
        catch
        {
            await WriteAuditAsync(
                audit,
                httpContext,
                AuditEvents.EmailChanged,
                AuditOutcome.Failed,
                actorId: null,
                subjectId: string.IsNullOrWhiteSpace(subjectId) ? null : subjectId,
                details: details);
            throw;
        }
    }

    private static async Task<string?> FindUserIdByEmailAsync(
        HttpContext httpContext,
        string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var userManager = httpContext.RequestServices
            .GetRequiredService<UserManager<ApplicationUser>>();
        return (await userManager.FindByEmailAsync(email))?.Id;
    }

    private static string? GetAuthenticatedUserId(HttpContext httpContext) =>
        httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        httpContext.User.FindFirstValue("sub");

    private static Task WriteAuditAsync(
        IAuditLogger audit,
        HttpContext httpContext,
        string eventName,
        AuditOutcome outcome,
        string? actorId,
        string? subjectId,
        AuditDetails? details = null)
    {
        return audit.LogAsync(
            new AuditEntry(
                eventName,
                outcome,
                actorId,
                subjectId,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.TraceIdentifier,
                details));
    }

    private static bool IsFailedResult(object? result)
    {
        while (result is INestedHttpResult nestedResult)
        {
            result = nestedResult.Result;
        }

        return result is IStatusCodeHttpResult { StatusCode: >= 400 };
    }
}
