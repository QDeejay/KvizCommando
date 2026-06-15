namespace KvizCommando.Server.Extensions;

public static class ProblemDetailsExtensions
{
    public static IServiceCollection AddAppProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails();
      
        return services;
    }
}
