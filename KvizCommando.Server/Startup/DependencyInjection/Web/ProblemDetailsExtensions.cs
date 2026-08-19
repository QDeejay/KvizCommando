namespace KvizCommando.Server.Startup;

public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Regisztrálja az egységes Problem Details hibaválaszokat.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    public static IServiceCollection AddAppProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails();
      
        return services;
    }
}
