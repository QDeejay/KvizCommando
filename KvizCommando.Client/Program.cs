using KvizCommando.Client;
using KvizCommando.Client.DependencyInjection;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services
    .AddClientLocalization()
    .AddClientState()
    .AddClientApiServices()
    .AddClientUiServices()
    .AddClientHttpPipeline(
        builder.HostEnvironment.BaseAddress);

var host = builder.Build();

await host.InitializeClientLocalizationAsync();
await host.RunAsync();
