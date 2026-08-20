using Blazored.LocalStorage;
using Blazored.SessionStorage;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Settings;

namespace KvizCommando.Client.DependencyInjection;

public static class ClientStateExtensions
{
    /// <summary>
    /// Regisztrálja a böngészőtárolókat, a kliens munkamenetét és a képernyők állapotpillanatképeit.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddClientState(
        this IServiceCollection services)
    {
        services.AddBlazoredSessionStorage();
        services.AddBlazoredLocalStorage();

        services.AddScoped<IHomeState, HomeState>();
        services.AddScoped<IQuestionState, QuestionState>();
        services.AddScoped<ITeamState, TeamState>();
        services.AddScoped<ISoloState, SoloState>();
        services.AddScoped<IVsState, VsState>();
        services.AddSingleton<SessionService>();
        services.AddScoped<ISettingsService, SettingsService>();

        return services;
    }
}
