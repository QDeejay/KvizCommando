using KvizCommando.Server.Endpoints;
using KvizCommando.Server.Extensions;
using KvizCommando.Server.Hubs;
using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Email;
using KvizCommando.Server.Infrastructure.Extensions;
using KvizCommando.Server.Infrastructure.Logging;
using KvizCommando.Server.Infrastructure.Options;
using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Server.Security.RateLimiting;
using KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Webes keretrendszer és állapottartó szolgáltatások
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
// Alkalmazásszolgáltatások
builder.Services.AddCustomServices();

// Háttérfolyamatok
builder.Services.AddBackgroundWorkers();

builder.Services.AddHttpContextAccessor();


// Adatbázis-kapcsolatok
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    // SQL Server alternatíva; a központi szolgáltatókapcsoló elkészültéig kikommentelve marad.
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    //     sqlOptions => sqlOptions.EnableRetryOnFailure());
});

builder.Services.AddDbContext<GameDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("GameConnection"));
    // SQL Server alternatíva; a központi szolgáltatókapcsoló elkészültéig kikommentelve marad.
    // options.UseSqlServer(builder.Configuration.GetConnectionString("GameConnection"),
    //     sqlOptions => sqlOptions.EnableRetryOnFailure());
});


// Biztonsági és személyesadat-kezelési réteg
builder.Services.AddSecurityAndPii(builder.Configuration);


// Hitelesítéshez kapcsolódó infrastruktúra
builder.Services.AddAppCors(builder.Configuration);
builder.Services.AddAppRateLimiting();
builder.Services.AddAppDataProtection(builder.Configuration, builder.Environment);
builder.Services.AddAppProblemDetails();
builder.Services.AddAppLocalization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.OperationFilter<SecurityRequirementsOperationFilter>();
});


builder.Services.AddTransient<IEmailSender<ApplicationUser>, WhitelistedEmailSender>();
builder.Services.Configure<AppOptions>(
    builder.Configuration.GetSection("App"));

// Identity
builder.Services.AddCustomIdentity(builder.Configuration);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

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
// Fejlesztési callback-diagnosztika. A principal naplózása miatt éles környezetben
// nem maradhat nyilvánosan elérhető; lezárását a docs/infrastructure-status.md rögzíti.
app.MapGet("/signin-facebook", async ctx =>
{
    var authRes = await ctx.AuthenticateAsync(IdentityConstants.ExternalScheme);
    Console.WriteLine(">>> SIGNIN FACEBOOK CALLBACK");
    Console.WriteLine("Succeeded: " + authRes.Succeeded);
    Console.WriteLine("Principal: " + authRes.Principal);
});

// Az endpoint kiválasztása megelőzi, a jogosultságvizsgálat pedig követi a hitelesítést.
app.UseAuthentication();
app.UseAuthorization();

// A diagnosztika mobil és asztali kliens tesztelésekor elkülöníti a cookie- és bearer-hibákat.
// Alapértelmezetten ki van kapcsolva, és tokent vagy felhasználói adatot nem naplóz.
if (builder.Configuration.GetValue<bool>("Diagnostics:EnableAuthenticationDebugLogging"))
{
    app.Use(async (context, next) =>
    {
        var cookieAuthentication = await context.AuthenticateAsync(
            IdentityConstants.ApplicationScheme);
        var bearerAuthentication = await context.AuthenticateAsync(
            IdentityConstants.BearerScheme);
        var hasBearerHeader = context.Request.Headers["Authorization"]
            .ToString()
            .StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

        app.Logger.LogInformation(
            "AUTH path={Path} cookie={CookieAuthenticated} bearerHeader={HasBearerHeader} bearer={BearerAuthenticated}",
            context.Request.Path,
            cookieAuthentication.Succeeded,
            hasBearerHeader,
            bearerAuthentication.Succeeded);

        await next();

        app.Logger.LogInformation(
            "AUTH response status={StatusCode} path={Path} endpoint={Endpoint}",
            context.Response.StatusCode,
            context.Request.Path,
            context.GetEndpoint()?.DisplayName ?? "<none>");
    });
}

app.UseRateLimiter();
app.UseExceptionHandler();

// Végpontok
app.MapRazorPages();


app.MapControllers();
app.MapHub<VsMatchHub>("/hubs/vs-match");
app.MapHub<SoloGameHub>("/hubs/solo-game");

// Identity API: bejelentkezés, regisztráció, megerősítés és jelszó-visszaállítás
app.MapGroup("/")
   .MapIdentityApi<ApplicationUser>()
   .WithPerEndpointRateLimiting()
   .WithIdentityAudit();
app.MapLogoutEndpoints();
app.MapFacebookAuthEndpoints();

app.MapFallbackToFile("index.html", clientAssetCacheOptions);

app.Run();
