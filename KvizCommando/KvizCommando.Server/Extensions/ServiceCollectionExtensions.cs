
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
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Server.Utilities.Recruit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;


namespace KvizCommando.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        ///
        /// Player
        /// 
       
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddScoped<IPlayerCacheService, PlayerCacheService>();
        services.AddScoped<IUserPlayerIdCacheService, UserPlayerIdCacheService>();

        services.AddSingleton<INameGenerator, NameGenerator>();
        services.AddSingleton<IPicCodeGenerator, PicCodeGenerator>();

        services.AddScoped<IRecruitService, RecruitService>();

        ///
        /// Dto services
        /// 
        services.AddScoped<IScreenService, ScreenService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<ITeamService, TeamService>();

        ///
        /// Game database services
        /// 
        services.AddScoped<IQuestionDbService, QuestionDbService>();
        services.AddScoped<IPlayerDbService, PlayerDbService>();

        ///
        /// Email services
        /// 
        services.AddScoped<ICallbackUrlValidator, CallbackUrlValidator>();
        ///
        /// services.AddScoped<IEmailLinkFactory, EmailLinkFactory>();
        /// 
        services.AddScoped<ITermsProvider, TermsProvider>();
        services.AddScoped<ICheckInService, CheckInService>();
        ///
        /// Auth services
        /// 
        services.AddScoped<IClaimsSyncService, ClaimsSyncService>();
        services.AddScoped<IAuthorizationHandler, TermsAcceptedHandler>();
        ///
        /// Logging services
        /// 
        services.AddScoped<IAuditLogger, AuditLogger>();

        /// Ideiglenes admin szolgáltatás
        /// 
        services.AddScoped<IAdminAppService, AdminAppService>();


        return services;
    }
}
