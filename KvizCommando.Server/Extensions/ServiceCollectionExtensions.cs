using KvizCommando.Server.Authorization;
using KvizCommando.Server.Infrastructure.Email;
using KvizCommando.Server.Infrastructure.Logging;
using KvizCommando.Server.Services;
using KvizCommando.Server.Services.Auth;
using KvizCommando.Server.Services.CheckIn;
using KvizCommando.Server.Services.Db;
using KvizCommando.Server.Services.DtoMapping;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.Players;
using KvizCommando.Server.Services.Security;
using KvizCommando.Server.Services.SoloGame;
using KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;
using KvizCommando.Server.Services.SoloGame.GameCache;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Server.Services.VsGame;
using KvizCommando.Server.Services.VsGame.Match;
using KvizCommando.Server.Services.VsGame.Matchmaking;
using Microsoft.AspNetCore.Authorization;


namespace KvizCommando.Server.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Regisztrálja az alkalmazás saját szolgáltatásait.
    /// </summary>
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        // Játékos- és munkamenetkezelés
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddScoped<IPlayerCacheService, PlayerCacheService>();
        services.AddScoped<IUserPlayerIdCacheService, UserPlayerIdCacheService>();

        // Képernyőadatok és játékmódok
        services.AddScoped<IScreenService, ScreenService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddSingleton<ISoloGameCache, SoloGameCache>();
        services.AddScoped<ISoloQuestionRepository, SoloQuestionRepository>();
        services.AddScoped<ISoloGameService, SoloGameService>();

        services.AddScoped<IVsGameService, VsGameService>();
        services.AddSingleton<VsMatchStore>();
        services.AddSingleton<IVsMatchQuestionLoader, VsMatchQuestionLoader>();
        services.AddSingleton<VsMatchSetupService>();
        services.AddSingleton<VsMatchRewardPersistenceService>();
        services.AddSingleton<IVsMatchService, VsMatchService>();
        services.AddSingleton<IVsRankedQueueService, VsRankedQueueService>();

        // Adatbázis-hozzáférés
        services.AddScoped<IQuestionDbService, QuestionDbService>();
        services.AddScoped<IPlayerDbService, PlayerDbService>();

        // Az egyéni játék kérdésindexe alkalmazásszintű gyorsítótár.
        services.AddSingleton<ICategoryQuestionIndexCache, CategoryQuestionIndexCache>();

        // E-mail callback címek ellenőrzése
        services.AddScoped<ICallbackUrlValidator, CallbackUrlValidator>();

        services.AddScoped<ITermsProvider, TermsProvider>();
        services.AddScoped<ICheckInService, CheckInService>();

        // Hitelesítés és jogosultságkezelés
        services.AddScoped<IClaimsSyncService, ClaimsSyncService>();
        services.AddScoped<IAuthorizationHandler, TermsAcceptedHandler>();

        // Auditnaplózás
        services.AddScoped<IAuditLogger, AuditLogger>();

        // A kérdésállomány karbantartására szolgáló adminisztrációs felület.
        services.AddScoped<IAdminAppService, AdminAppService>();

        return services;
    }
}
