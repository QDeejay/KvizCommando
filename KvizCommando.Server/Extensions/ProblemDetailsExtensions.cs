namespace KvizCommando.Server.Extensions;

public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Regisztrálja az egységes Problem Details hibaválaszokat.
    /// </summary>
    public static IServiceCollection AddAppProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails();
      
        return services;
    }
}
