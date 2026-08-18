using Blazored.LocalStorage;
using Blazored.SessionStorage;
using KvizCommando.Client;
using KvizCommando.Client.Features.Question.Services;
using KvizCommando.Client.Features.Solo.Services;
using KvizCommando.Client.Features.Team.Services;
using KvizCommando.Client.Features.VsGame.Services;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Http;
using KvizCommando.Client.Services;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Services.Visual.UiService.Language;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<ILanguageService, LanguageService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICacheApiService, CacheApiService>();
builder.Services.AddScoped<IQuestionClientService, QuestionClientService>();
builder.Services.AddScoped<ISoloGameClientService, SoloGameClientService>();
builder.Services.AddScoped<ITeamClientService, TeamClientService>();
builder.Services.AddScoped<IVsGameClientService, VsGameClientService>();
builder.Services.AddScoped<IVsMatchClientService, VsMatchClientService>();

builder.Services.AddScoped<IHomeState, HomeState>();
builder.Services.AddScoped<IQuestionState, QuestionState>();
builder.Services.AddScoped<ITeamState, TeamState>();
builder.Services.AddScoped<ISoloState, SoloState>();
builder.Services.AddScoped<IVsState, VsState>();

builder.Services.AddSingleton<IDisplayMessageState, DisplayMessageState>();
builder.Services.AddScoped<PageHeaderService>();
builder.Services.AddScoped<ModalService>();
builder.Services.AddSingleton<ToastService>();
builder.Services.AddSingleton<SubHeaderService>();
builder.Services.AddScoped<UiServices>();
builder.Services.AddScoped<MarkupLoaderService>();

builder.Services.AddScoped<CategoryOptionHelpers>();
builder.Services.AddSingleton<AudioService>();
builder.Services.AddSingleton<LoaderService>();
builder.Services.AddSingleton<SessionService>();
builder.Services.AddTransient<LoaderHandler>();
builder.Services.AddTransient<LanguageHandler>();
builder.Services.AddTransient<AuthRedirectHandler>();
builder.Services.AddTransient<ToastHandler>();
builder.Services.AddTransient<LoggingHandler>();

builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var client = factory.CreateClient("DefaultWithLang");
    return client;
});

builder.Services.AddHttpClient("DefaultWithLang", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
})
.AddHttpMessageHandler<LoaderHandler>()
.AddHttpMessageHandler<LanguageHandler>()
.AddHttpMessageHandler<AuthRedirectHandler>()
.AddHttpMessageHandler<ToastHandler>()
.AddHttpMessageHandler<LoggingHandler>();


builder.Services.AddSingleton<ICategoryLookupService, StaticCategoryLookupService>();

builder.Services.AddScoped<IdentityRulesService>();
builder.Services.AddBlazoredSessionStorage();



///
/// Version 1.026.0621.01

/// 



await builder.Build().RunAsync();
