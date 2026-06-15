using Blazored.LocalStorage;
using Blazored.SessionStorage;
using KvizCommando.Client;
using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<ILoadingService, LoadingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<CategoryOptionHelpers>();
builder.Services.AddScoped<DevDataBuilder>();
builder.Services.AddScoped<UpperBlockDataBuilder>();
builder.Services.AddScoped<BottomBlockDataBuilder>();
builder.Services.AddScoped<RecruitBlockBuilder>();
builder.Services.AddSingleton<AudioService>();

builder.Services.AddScoped<ITeamModalDataBuilder, TeamModalDataBuilder>();

// ...
builder.Services.AddScoped<UiHeaderState>();
// ...



/*
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient
    {
        BaseAddress = new Uri(navigationManager.BaseUri)
    };
});
builder.Services.AddTransient<LoggingHandler>();

builder.Services.AddScoped(sp =>
{
    var navigation = sp.GetRequiredService<NavigationManager>();

    var loggingHandler = new LoggingHandler
    {
        InnerHandler = new HttpClientHandler() // Itt NEM kell UseCookies, mert WASM-ban úgysem támogatott
    };

    return new HttpClient(loggingHandler)
    {
        BaseAddress = new Uri(navigation.BaseUri)
    };
});
*/
builder.Services.AddTransient<LanguageHandler>();
builder.Services.AddTransient<AuthRedirectHandler>();
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
.AddHttpMessageHandler<LanguageHandler>()
.AddHttpMessageHandler<LoggingHandler>()
.AddHttpMessageHandler<AuthRedirectHandler>();


builder.Services.AddSingleton<IDisplayMessageState, DisplayMessageState>();
builder.Services.AddScoped<IScreenApiService, ScreenApiService>();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IHomeState, HomeState>();
builder.Services.AddScoped<IQuestionState, QuestionState>();
builder.Services.AddScoped<ITeamState, TeamState>();
builder.Services.AddScoped<ISoloState, SoloState>();
builder.Services.AddSingleton<ICategoryLookupService, StaticCategoryLookupService>();

builder.Services.AddScoped<PageTitleService>();
builder.Services.AddScoped<IdentityRulesService>();
builder.Services.AddBlazoredSessionStorage();

await builder.Build().RunAsync();

