using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BWin2.Wasm;
using BWin2.Wasm.Data;
using BWin2.Wasm.Services;
using BWin2.Wasm.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    });
builder.Services.AddScoped<IRandomSource, QBasicRandom>();
builder.Services.AddScoped<IGameDataStore, WasmGameDataStore>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IOddsService, OddsService>();
builder.Services.AddScoped<IBettingService, BettingService>();
builder.Services.AddScoped<ILeagueService, LeagueService>();
builder.Services.AddScoped<ICommentaryScriptService, CommentaryScriptService>();
builder.Services.AddScoped<ISeasonProgressService, SeasonProgressService>();
builder.Services.AddScoped<IMatchPresentation, MatchPresentation>();
builder.Services.AddScoped<IMatchEngine, MatchEngine>();
builder.Services.AddScoped<IGameSession, GameSession>();

await builder.Build().RunAsync();
