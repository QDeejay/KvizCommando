#nullable enable
using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Logging;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.Players;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KvizCommando.Server.Startup;

public static class LogoutEndpoints
{
    /// <summary>
    /// Regisztrálja a kijelentkezési végpontokat.
    /// </summary>
    /// <param name="routes">A végpontokat fogadó útvonalcsoport.</param>
    public static IEndpointRouteBuilder MapLogoutEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/logout")
            .WithTags("Auth")
            .RequireAuthorization("Api"); // minden logout auth-ot igényel
           
        // POST /api/logout
        group.MapPost("", async (
            [FromBody] string sessionId,
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IPlayerService playerService,
            IAuditLogger audit,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? httpContext.User.FindFirstValue("sub");
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrWhiteSpace(userId))
            {
                await audit.LogAsync(
                    new AuditEntry(
                        AuditEvents.Logout,
                        AuditOutcome.Failed,
                        ActorId: null,
                        SubjectId: null,
                        IpAddress: ipAddress,
                        RequestId: httpContext.TraceIdentifier));
                return Results.Unauthorized();
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                await audit.LogAsync(
                    new AuditEntry(
                        AuditEvents.Logout,
                        AuditOutcome.Failed,
                        ActorId: null,
                        SubjectId: userId,
                        IpAddress: ipAddress,
                        RequestId: httpContext.TraceIdentifier));
                return Results.Unauthorized();
            }

            var sessionStatus = await playerService.CheckSessionAsync(
                userId,
                sessionId,
                ct);

            if (sessionStatus == CacheReadStatus.SessionMismatch)
            {
                await audit.LogAsync(
                    new AuditEntry(
                        AuditEvents.Logout,
                        AuditOutcome.Denied,
                        userId,
                        userId,
                        ipAddress,
                        httpContext.TraceIdentifier));
                return Results.Conflict();
            }

            var hasBearer = httpContext.Request.Headers.Authorization
                .ToString()
                .StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

            if (hasBearer)
            {
                // Bearer kijelentkezésnél a security stamp frissítése minden kiadott tokent érvénytelenít.
                var stampResult = await userManager.UpdateSecurityStampAsync(user);
                if (!stampResult.Succeeded)
                {
                    await audit.LogAsync(
                        new AuditEntry(
                            AuditEvents.SessionRevoked,
                            AuditOutcome.Failed,
                            userId,
                            userId,
                            ipAddress,
                            httpContext.TraceIdentifier));
                    await audit.LogAsync(
                        new AuditEntry(
                            AuditEvents.Logout,
                            AuditOutcome.Failed,
                            userId,
                            userId,
                            ipAddress,
                            httpContext.TraceIdentifier));
                    return Results.Problem(
                        "session_revoke_failed",
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            }
            else
            {
                await signInManager.SignOutAsync();
            }

            Console.WriteLine($"Kijelentkezés User{userId} Session:{sessionId}");
            await playerService.LogoutAndRemoveCacheAsync(userId, sessionId, ct);

            if (hasBearer)
            {
                await audit.LogAsync(
                    new AuditEntry(
                        AuditEvents.SessionRevoked,
                        AuditOutcome.Succeeded,
                        userId,
                        userId,
                        ipAddress,
                        httpContext.TraceIdentifier));
            }

            await audit.LogAsync(
                new AuditEntry(
                    AuditEvents.Logout,
                    AuditOutcome.Succeeded,
                    userId,
                    userId,
                    ipAddress,
                    httpContext.TraceIdentifier));

            return Results.NoContent();
        });

        return routes;
    }
}
