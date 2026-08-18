using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Logging;
using KvizCommando.Server.Services.Db;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace KvizCommando.Server.Startup;

public static class FacebookAuthEndpoints
{
    /// <summary>
    /// Regisztrálja a Facebook-hitelesítés végpontjait.
    /// </summary>
    /// <param name="app">A konfigurálandó alkalmazás vagy végpontépítő.</param>
    public static IEndpointRouteBuilder MapFacebookAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // A külső szolgáltató callbackje a befejező végpontra tér vissza.
        app.MapGet("/login/facebook", async (
             SignInManager<ApplicationUser> signInManager,
             HttpContext ctx) =>
        {
            var props = signInManager
                .ConfigureExternalAuthenticationProperties("Facebook", "/finished");
            await ctx.ChallengeAsync("Facebook", props);
        });
        app.MapGet("/finished", async (
                SignInManager<ApplicationUser> signInManager,
                UserManager<ApplicationUser> userManager,
                IPlayerDbService playerDb,
                IAuditLogger audit,
                HttpContext ctx) =>
        {
            var qs = ctx.Request.QueryString.Value;
            var uriReturn = $"/checkin?name={Uri.EscapeDataString("OK")}";
            if (!string.IsNullOrEmpty(qs) && qs.Contains("error=", StringComparison.OrdinalIgnoreCase))
            {
                await ctx.SignOutAsync(IdentityConstants.ExternalScheme);
                return Results.Redirect("/" + qs);
            }


            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                await audit.LogAsync(
                    new AuditEntry(
                        AuditEvents.LOGIN,
                        AuditOutcome.Failed,
                        ActorId: null,
                        SubjectId: null,
                        IpAddress: ctx.Connection.RemoteIpAddress?.ToString(),
                        RequestId: ctx.TraceIdentifier));
                return Results.Redirect("/?error=NoInfo");
            }

            var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            // Meglévő e-mail-cím esetén a külső azonosító a már létező fiókhoz kapcsolódik.
            if (user == null)
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email)
                            ?? $"fb_{info.ProviderKey}@example.com";

                user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        // A külső szolgáltató által átadott e-mail-cím jelenleg megerősítettnek minősül.
                        // A bizalmi szint megváltoztatása regisztrációs és fiók-összekapcsolási döntés is.
                        EmailConfirmed = true
                    };
                    var cr = await userManager.CreateAsync(user);
                    if (!cr.Succeeded)
                    {
                        await audit.LogAsync(
                            new AuditEntry(
                                AuditEvents.ACCOUNT_REGISTERED,
                                AuditOutcome.Failed,
                                ActorId: null,
                                SubjectId: null,
                                IpAddress: ctx.Connection.RemoteIpAddress?.ToString(),
                                RequestId: ctx.TraceIdentifier));
                        await audit.LogAsync(
                            new AuditEntry(
                                AuditEvents.LOGIN,
                                AuditOutcome.Failed,
                                ActorId: null,
                                SubjectId: null,
                                IpAddress: ctx.Connection.RemoteIpAddress?.ToString(),
                                RequestId: ctx.TraceIdentifier));
                        return Results.Redirect("/?error=CreateFailed");
                    }
                    await audit.LogAsync(
                        new AuditEntry(
                            AuditEvents.ACCOUNT_REGISTERED,
                            AuditOutcome.Succeeded,
                            user.Id,
                            user.Id,
                            ctx.Connection.RemoteIpAddress?.ToString(),
                            ctx.TraceIdentifier));
                    var FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                    var SuggestedName = await playerDb.SuggestAsync(FirstName);
                    uriReturn = $"/checkin?name={Uri.EscapeDataString(SuggestedName)}";
                }

                var lr = await userManager.AddLoginAsync(user, info);
                if (!lr.Succeeded)
                {
                    await audit.LogAsync(
                        new AuditEntry(
                            AuditEvents.EXTERNAL_LOGIN_LINKED,
                            AuditOutcome.Failed,
                            ActorId: null,
                            SubjectId: user.Id,
                            IpAddress: ctx.Connection.RemoteIpAddress?.ToString(),
                            RequestId: ctx.TraceIdentifier));
                    await audit.LogAsync(
                        new AuditEntry(
                            AuditEvents.LOGIN,
                            AuditOutcome.Failed,
                            ActorId: null,
                            SubjectId: user.Id,
                            IpAddress: ctx.Connection.RemoteIpAddress?.ToString(),
                            RequestId: ctx.TraceIdentifier));
                    return Results.Redirect("/?error=LinkFailed");
                }
                await audit.LogAsync(
                    new AuditEntry(
                        AuditEvents.EXTERNAL_LOGIN_LINKED,
                        AuditOutcome.Succeeded,
                        user.Id,
                        user.Id,
                        ctx.Connection.RemoteIpAddress?.ToString(),
                        ctx.TraceIdentifier));
            }

            // Csak a fiók e-mail-címével pontosan egyező külső claim igazolhatja a címet.
            if (!user.EmailConfirmed)
            {
                var claimEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrWhiteSpace(claimEmail) &&
                    string.Equals(claimEmail, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    await userManager.ConfirmEmailAsync(user, code);
                }
            }

            await signInManager.SignInAsync(user, isPersistent: false);
            await signInManager.UpdateExternalAuthenticationTokensAsync(info);
            await ctx.SignOutAsync(IdentityConstants.ExternalScheme);

            var ipAddress = ctx.Connection.RemoteIpAddress?.ToString();
            await audit.LogAsync(
                new AuditEntry(
                    AuditEvents.LOGIN,
                    AuditOutcome.Succeeded,
                    user.Id,
                    user.Id,
                    ipAddress,
                    ctx.TraceIdentifier));


            return Results.Redirect(uriReturn);
        });
        // Az alkalmazás eltávolításakor a külső bejelentkezés és minden Facebook-token törlendő.
        app.MapPost("/facebook/deauthorize", async (
            [FromForm] string signed_request,
            IConfiguration config,
            UserManager<ApplicationUser> userManager,
            IAuditLogger audit,
            HttpContext ctx) =>
        {
            if (string.IsNullOrWhiteSpace(signed_request))
                return Results.BadRequest(new { error = "missing_signed_request" });

            var appSecret = config["Authentication:Facebook:AppSecret"];
            if (string.IsNullOrWhiteSpace(appSecret))
                return Results.BadRequest(new { error = "missing_app_secret" });

            if (!TryVerifyAndDecodeSignedRequest(signed_request, appSecret, out var payload) ||
                !payload.TryGetProperty("user_id", out var uid) ||
                string.IsNullOrWhiteSpace(uid.GetString()))
            {
                return Results.BadRequest(new { error = "invalid_signed_request" });
            }

            var fbUserId = uid.GetString()!;
            var user = await userManager.FindByLoginAsync("Facebook", fbUserId);
            if (user != null)
            {
                var removeResult = await userManager.RemoveLoginAsync(user, "Facebook", fbUserId);
                await userManager.RemoveAuthenticationTokenAsync(user, "Facebook", "access_token");
                await userManager.RemoveAuthenticationTokenAsync(user, "Facebook", "expires_at");
                await userManager.RemoveAuthenticationTokenAsync(user, "Facebook", "token_type");

                await audit.LogAsync(
                    new AuditEntry(
                        AuditEvents.EXTERNAL_LOGIN_REMOVED,
                        removeResult.Succeeded
                            ? AuditOutcome.Succeeded
                            : AuditOutcome.Failed,
                        ActorId: null,
                        SubjectId: user.Id,
                        IpAddress: ctx.Connection.RemoteIpAddress?.ToString(),
                        RequestId: ctx.TraceIdentifier));
            }
            return Results.Ok(new { status = "ok" });
        }).AllowAnonymous();

        // A Meta adateltávolítási callbackje visszakövethető állapotcímet és megerősítő kódot vár.
        app.MapPost("/facebook/deletion", (
            HttpContext ctx,
            [FromForm] string signed_request,
            IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(signed_request))
                return Results.BadRequest(new { error = "missing_signed_request" });

            var appSecret = config["Authentication:Facebook:AppSecret"];
            if (string.IsNullOrWhiteSpace(appSecret))
                return Results.BadRequest(new { error = "missing_app_secret" });

            if (!TryVerifyAndDecodeSignedRequest(signed_request, appSecret, out _))
                return Results.BadRequest(new { error = "invalid_signed_request" });

            var code = Guid.NewGuid().ToString("N");
            var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
            var statusUrl = $"{baseUrl}/privacy/data-deletion?code={code}";
            return Results.Ok(new { url = statusUrl, confirmation_code = code });
        }).AllowAnonymous();

        return app;
    }

    private static bool TryVerifyAndDecodeSignedRequest(string signedRequest, string appSecret, out JsonElement payload)
    {
        payload = default;

        var parts = signedRequest.Split('.', 2);
        if (parts.Length != 2) return false;

        var sig = Base64UrlDecode(parts[0]);
        var payloadBytes = Base64UrlDecode(parts[1]);
        if (sig is null || payloadBytes is null) return false;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret));
        var expected = hmac.ComputeHash(Encoding.UTF8.GetBytes(parts[1]));
        if (!CryptographicOperations.FixedTimeEquals(sig, expected)) return false;

        try
        {
            using var doc = JsonDocument.Parse(payloadBytes);
            // A klónozás leválasztja az eredményt a metódus végén felszabaduló dokumentumról.
            payload = doc.RootElement.Clone();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static byte[]? Base64UrlDecode(string input)
    {
        input = input.Replace('-', '+').Replace('_', '/');
        switch (input.Length % 4)
        {
            case 2: input += "=="; break;
            case 3: input += "="; break;
        }
        try { return Convert.FromBase64String(input); } catch { return null; }
    }
}
