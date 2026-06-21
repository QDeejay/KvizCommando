// src/Server/Endpoints/LogoutEndpoints.cs
#nullable enable
using KvizCommando.Server.Identity;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.Players;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace KvizCommando.Server.Endpoints;

public static class LogoutEndpoints
{
    public static IEndpointRouteBuilder MapLogoutEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/logout")
            .WithTags("Auth")
            .RequireAuthorization("Api"); // minden logout auth-ot igényel
           
        // POST /api/logout
        group.MapPost("", async (
            string? sessionId,
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IPlayerService playerService,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? httpContext.User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            // Döntés: Bearer vagy Cookie?
            var hasBearer = httpContext.Request.Headers.Authorization
                .ToString()
                .StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

            if (hasBearer)
            {
                // Bearer → SecurityStamp frissítés (összes session kill)
                await userManager.UpdateSecurityStampAsync(user);
            }
            else
            {
                // Cookie → gyári sign out
                await signInManager.SignOutAsync();
            }

            // TODO: cache invalidálás (ha lesz PlayerCache)
            // PlayerCache.Remove(user.Id);
            Console.WriteLine($"Kijelentkezés User{userId} Session:{sessionId}");
            await playerService.LogoutAndRemoveCacheAsync(userId, sessionId, ct);

            return Results.NoContent();
        });

        return routes;
    }
}
