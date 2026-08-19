using KvizCommando.Client.Features.Question.Services;
using KvizCommando.Client.Features.Solo.Services;
using KvizCommando.Client.Features.Team.Services;
using KvizCommando.Client.Features.VsGame.Services;
using KvizCommando.Client.Services;
using KvizCommando.Client.Services.ScreenData;
using KvizCommando.Client.Services.User;

namespace KvizCommando.Client.DependencyInjection;

public static class ClientApiServiceExtensions
{
    /// <summary>
    /// Regisztrálja a hitelesítési, képernyőadat- és játékmód-specifikus API-klienseket.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddClientApiServices(
        this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICacheApiService, CacheApiService>();
        services.AddScoped<IQuestionClientService, QuestionClientService>();
        services.AddScoped<ISoloGameClientService, SoloGameClientService>();
        services.AddScoped<ITeamClientService, TeamClientService>();
        services.AddScoped<IVsGameClientService, VsGameClientService>();
        services.AddScoped<IVsMatchClientService, VsMatchClientService>();
        services.AddScoped<IdentityRulesService>();

        return services;
    }
}
