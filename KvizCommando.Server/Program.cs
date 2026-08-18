using KvizCommando.Server.Startup;
using KvizCommando.Server.Hubs;
using KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddKvizCommandoWeb()
    .AddKvizCommandoPersistence(builder.Configuration)
    .AddKvizCommandoIdentity(builder.Configuration)
    .AddKvizCommandoSecurity(
        builder.Configuration,
        builder.Environment)
    .AddKvizCommandoGameplay()
    .AddKvizCommandoBackgroundWorkers();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}

var app = builder.Build();

var categoryQuestionIndexCache =
    app.Services.GetRequiredService<ICategoryQuestionIndexCache>();

await categoryQuestionIndexCache.LoadAsync();

// Fejlesztői eszközök
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

// Lokalizáció
app.UseAppLocalization("hu-HU", new[] { "hu-HU", "en-US" });

// HTTP-feldolgozási lánc

var clientAssetCacheOptions = new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        var extension = Path.GetExtension(
            context.File.Name);

        if (extension is ".html" or ".js" or ".css" or ".json")
        {
            context.Context.Response.Headers.CacheControl =
                "no-cache, must-revalidate";
        }
    }
};

app.UseBlazorFrameworkFiles();
app.UseStaticFiles(clientAssetCacheOptions);

app.UseRouting();

app.UseCors("Spa");
// Az endpoint kiválasztása megelőzi, a jogosultságvizsgálat pedig követi a hitelesítést.
app.UseAuthentication();
app.UseAuthorization();

// A diagnosztika mobil és asztali kliens tesztelésekor elkülöníti a cookie- és bearer-hibákat.
// Alapértelmezetten ki van kapcsolva, és tokent vagy felhasználói adatot nem naplóz.
app.UseAuthenticationDiagnostics(builder.Configuration);

app.UseRateLimiter();

// Végpontok
app.MapRazorPages();
app.MapControllers();
app.MapHub<VsMatchHub>("/hubs/vs-match");
app.MapHub<SoloGameHub>("/hubs/solo-game");

app.MapKvizCommandoIdentityEndpoints();

app.MapFallbackToFile("index.html", clientAssetCacheOptions);

app.Run();
