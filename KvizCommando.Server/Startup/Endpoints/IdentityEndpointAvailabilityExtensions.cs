using KvizCommando.Server.Identity;

namespace KvizCommando.Server.Startup;

public static class IdentityEndpointAvailabilityExtensions
{
    /// <summary>
    /// Enforces operational availability switches on the built-in Identity endpoints.
    /// </summary>
    public static IEndpointConventionBuilder WithPublicAuthAvailability(
        this IEndpointConventionBuilder builder,
        IConfiguration configuration)
    {
        var registrationEnabled =
            IdentityConfiguration.IsRegistrationEnabled(configuration);

        builder.AddEndpointFilter(async (context, next) =>
        {
            if (!registrationEnabled &&
                HttpMethods.IsPost(context.HttpContext.Request.Method) &&
                string.Equals(
                    context.HttpContext.Request.Path.Value,
                    "/register",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            return await next(context);
        });

        return builder;
    }
}
