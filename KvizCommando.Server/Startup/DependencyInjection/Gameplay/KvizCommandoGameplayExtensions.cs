using KvizCommando.Server.Services;
using KvizCommando.Server.Services.DtoMapping;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.Players;
using KvizCommando.Server.Services.Profile;
using KvizCommando.Server.Services.SoloGame;
using KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;
using KvizCommando.Server.Services.SoloGame.GameCache;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Server.Services.VsGame;
using KvizCommando.Server.Services.VsGame.Match;
using KvizCommando.Server.Services.VsGame.Matchmaking;

namespace KvizCommando.Server.Startup;

public static class KvizCommandoGameplayExtensions
{
    /// <summary>
    /// Regisztrálja a játékosállapot, a képernyők és a játékmódok alkalmazásszolgáltatásait.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddKvizCommandoGameplay(
        this IServiceCollection services)
    {
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IProfileAccountService, ProfileAccountService>();
        services.AddScoped<IPlayerCacheService, PlayerCacheService>();
        services.AddScoped<IUserPlayerIdCacheService, UserPlayerIdCacheService>();

        services.AddScoped<IScreenService, ScreenService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<ITeamService, TeamService>();

        services.AddSingleton<ISoloGameCache, SoloGameCache>();
        services.AddScoped<ISoloQuestionRepository, SoloQuestionRepository>();
        services.AddScoped<ISoloGameService, SoloGameService>();
        services.AddSingleton<ICategoryQuestionIndexCache, CategoryQuestionIndexCache>();

        services.AddScoped<IVsGameService, VsGameService>();
        services.AddSingleton<VsMatchStore>();
        services.AddSingleton<IVsMatchQuestionLoader, VsMatchQuestionLoader>();
        services.AddSingleton<VsMatchSetupService>();
        services.AddSingleton<VsMatchRewardPersistenceService>();
        services.AddSingleton<IVsMatchService, VsMatchService>();
        services.AddSingleton<IVsRankedQueueService, VsRankedQueueService>();

        services.AddScoped<IAdminAppService, AdminAppService>();

        return services;
    }
}
