using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace KvizCommando.Server.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Globális védelem – nagyon laza, csak flood ellen
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(ctx =>
            {
                var ip = ctx.Connection.RemoteIpAddress ?? IPAddress.IPv6None;
                return RateLimitPartition.GetTokenBucketLimiter(ip, _ => new TokenBucketRateLimiterOptions
                {
                    AutoReplenishment = true,
                    TokenLimit = 300,
                    TokensPerPeriod = 300,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });

            // Regisztráció és e-mail megerősítés elleni burst limit
            options.AddPolicy("signup-burst", ctx =>
            {
                var ip = ctx.Connection.RemoteIpAddress ?? IPAddress.IPv6None;
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });

            // Elfelejtett jelszó / reset flow védelme
            options.AddPolicy("password-reset", ctx =>
            {
                var ip = ctx.Connection.RemoteIpAddress ?? IPAddress.IPv6None;
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 3,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });

            // Login brute force védelem
            options.AddPolicy("login-protect", ctx =>
            {
                var ip = ctx.Connection.RemoteIpAddress ?? IPAddress.IPv6None;
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });
        });

        return services;
    }
}
