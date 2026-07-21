using KvizCommando.Server.Endpoints;
using KvizCommando.Server.Extensions;
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

// --- MVC + Razor ---
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
// --- Saját szolgáltatások ---
builder.Services.AddCustomServices();

// --- Background szolgáltatások ---
builder.Services.AddBackgroundWorkers();

builder.Services.AddHttpContextAccessor();


// --- EF Core ---
var provider = builder.Configuration["DatabaseProvider"];

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    // SQL Server verzió:
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    //     sqlOptions => sqlOptions.EnableRetryOnFailure());
});

builder.Services.AddDbContext<GameDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("GameConnection"));
    // SQL Server verzió:
    // options.UseSqlServer(builder.Configuration.GetConnectionString("GameConnection"),
    //     sqlOptions => sqlOptions.EnableRetryOnFailure());
});


// PII + security réteg
builder.Services.AddSecurityAndPii(builder.Configuration);


// --- Authentikáció + autorizáció ---
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

// --- Identity ---
builder.Services.AddCustomIdentity(builder.Configuration);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

var categoryQuestionIndexCache =
    app.Services.GetRequiredService<ICategoryQuestionIndexCache>();

await categoryQuestionIndexCache.LoadAsync();




// --- Diagnosztikai log ---
/*
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    var isAuth = context.User?.Identity?.IsAuthenticated ?? false;

    Console.WriteLine($"[DIAG] {context.Request.Method} {context.Request.Path} | " +
                      $"AuthHeader: {authHeader ?? "(none)"} | " +
                      $"IsAuthenticated: {isAuth}");

    await next();
});
*/

// --- Dev eszközök ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
    app.UseDeveloperExceptionPage();
}

// --- Lokalizáció ---
app.UseAppLocalization("hu-HU", new[] { "hu-HU", "en-US" });

// --- Middleware lánc ---
//app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("Spa");
app.MapGet("/signin-facebook", async ctx =>
{
    var authRes = await ctx.AuthenticateAsync(IdentityConstants.ExternalScheme);
    Console.WriteLine(">>> SIGNIN FACEBOOK CALLBACK");
    Console.WriteLine("Succeeded: " + authRes.Succeeded);
    Console.WriteLine("Principal: " + authRes.Principal);
});

app.UseAuthentication();   // <<< fontos: routing után
app.UseAuthorization();



/*
// Üzenet fejléc elennőrzés - diagnosztika
app.Use(async (ctx, next) =>
{
    var cookieRes = await ctx.AuthenticateAsync(IdentityConstants.ApplicationScheme);
    var bearerRes = await ctx.AuthenticateAsync(IdentityConstants.BearerScheme);
    var hasBearerHdr = ctx.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

   app.Logger.LogInformation("AUTH path={path} cookieOK={cOK} bearerHdr={bh} bearerOK={bOK}",
        ctx.Request.Path, cookieRes.Succeeded, hasBearerHdr, bearerRes.Succeeded);

    await next();

    var ep = ctx.GetEndpoint();
    app.Logger.LogInformation("RESP {status} path={path} ep={ep}",
        ctx.Response.StatusCode, ctx.Request.Path, ep?.DisplayName ?? "<none>");
});
*/

app.Use(async (ctx, next) =>
{
    var hdr = ctx.Request.Headers["Authorization"].ToString();
    var cookieAuth = await ctx.AuthenticateAsync(IdentityConstants.ApplicationScheme);
    //var bearerAuth = await ctx.AuthenticateAsync("Bearer");
    var idBearerAuth = await ctx.AuthenticateAsync(IdentityConstants.BearerScheme);
    //Console.Clear();
    //Console.SetCursorPosition(0, 0);
    app.Logger.LogWarning(@"
=== AUTH DEBUG ===
PATH: {path}
HEADER: {hdr}
CookieAuth: {cOK}
IdentityBearerAuth: {ibOK}
User.Identity.IsAuthenticated: {auth}
Name: {name}
==================",
        ctx.Request.Path,
        hdr,
        cookieAuth.Succeeded,
        //bearerAuth.Succeeded,
        idBearerAuth.Succeeded,
        ctx.User?.Identity?.IsAuthenticated ?? false,
        ctx.User?.Identity?.Name ?? "(null)");

    await next();
});

app.UseRateLimiter();
app.UseExceptionHandler();

//app.MapCheckInEndpoints();
// --- Endpointok ---
app.MapRazorPages();


app.MapControllers();

// Identity API endpointok (login, register, confirm, reset)
app.MapGroup("/")
   .MapIdentityApi<ApplicationUser>()   // gyári .NET 8 login/register/refresh
   .WithPerEndpointRateLimiting()
   .WithIdentityAudit();
app.MapLogoutEndpoints();
app.MapFacebookAuthEndpoints();

app.MapFallbackToFile("index.html");

app.Run();

