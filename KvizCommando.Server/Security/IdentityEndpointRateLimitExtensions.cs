using Microsoft.AspNetCore.RateLimiting;

namespace KvizCommando.Server.Security.RateLimiting;

public static class IdentityEndpointRateLimitExtensions
{
    public static IEndpointConventionBuilder WithPerEndpointRateLimiting(this IEndpointConventionBuilder builder)
    {
        builder.Add(endpointBuilder =>
        {
            var pattern = endpointBuilder.DisplayName?.ToLowerInvariant() ?? string.Empty;

            if (pattern.Contains("/register") || pattern.Contains("/resendconfirmationemail"))
            {
                endpointBuilder.Metadata.Add(new EnableRateLimitingAttribute("signup-burst"));
            }
            else if (pattern.Contains("/forgotpassword") || pattern.Contains("/resetpassword"))
            {
                endpointBuilder.Metadata.Add(new EnableRateLimitingAttribute("password-reset"));
            }
            else if (pattern.Contains("/login"))
            {
                endpointBuilder.Metadata.Add(new EnableRateLimitingAttribute("login-protect"));
            }
        });

        return builder;
    }
}
