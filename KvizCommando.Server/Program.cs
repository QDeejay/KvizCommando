using KvizCommando.Server.Hubs;
using KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;
using KvizCommando.Server.Startup;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "secrets.json",
    optional: true,
    reloadOnChange: true);

//  Sql server migrációs utasítások: Migration -custom elnevezés-
/*
     Add-Migration InitialSqlServerApplication -Context SqlServerApplicationDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -OutputDir Data/Migrations/SqlServer/Application -Args "--Database:Provider=SqlServer"

     Add-Migration InitialSqlServerGame -Context SqlServerGameDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -OutputDir Data/Migrations/SqlServer/Game -Args "--Database:Provider=SqlServer"

     Update-Database -Context SqlServerApplicationDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -Args "--Database:Provider=SqlServer"

     Update-Database -Context SqlServerGameDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -Args "--Database:Provider=SqlServer" 
*/

// SQLite migrációs utasítások: Migration -custom elnevezés-
/*
     Add-Migration InitialSqliteApplication -Context SqliteApplicationDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -OutputDir Data/Migrations/Sqlite/Application -Args "--Database:Provider=Sqlite"

     Add-Migration InitialSqliteGame -Context SqliteGameDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -OutputDir Data/Migrations/Sqlite/Game -Args "--Database:Provider=Sqlite"

     Update-Database -Context SqliteApplicationDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -Args "--Database:Provider=Sqlite"

     Update-Database -Context SqliteGameDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -Args "--Database:Provider=Sqlite"
 */

// Az EF migráció miatt kell itt a fejlesztői környezetben, migraciós parancsok futtatásához. A tényleges adatbázis-kapcsolatot a konfigurációs fájlokban és a parancssori argumentumokban lehet felülírni.
builder.Configuration.AddCommandLine(args);

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

// az Ngork-os ideiglenes domain név miatt a ForwardedHeaders beállításokat kell alkalmazni, hogy a helyes protokollt és IP-címet kapjuk meg a kérésekből ellenkező esetben vissza dobja a kérést
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();

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

app.MapKvizCommandoIdentityEndpoints(builder.Configuration);

app.MapFallbackToFile("index.html", clientAssetCacheOptions);

app.Run();
