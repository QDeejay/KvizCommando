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
        app.MapGet("/login/facebook", (
            SignInManager<ApplicationUser> signInManager,
            HttpContext ctx) =>
            StartFacebookLoginAsync(signInManager, ctx));
        app.MapGet("/finished", (
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IPlayerDbService playerDb,
            IAuditLogger audit,
            HttpContext ctx) =>
            FinishFacebookLoginAsync(
                signInManager,
                userManager,
                playerDb,
                audit,
                ctx));

        // Az alkalmazás eltávolításakor a külső bejelentkezés és minden Facebook-token törlendő.
        app.MapPost("/facebook/deauthorize", (
            [FromForm] string signed_request,
            IConfiguration config,
            UserManager<ApplicationUser> userManager,
            IAuditLogger audit,
            HttpContext ctx) =>
            DeauthorizeFacebookAsync(
                signed_request,
                config,
                userManager,
                audit,
                ctx)).AllowAnonymous();

        // A Meta adateltávolítási callbackje visszakövethető állapotcímet és megerősítő kódot vár.
        app.MapPost("/facebook/deletion", (
            HttpContext ctx,
            [FromForm] string signed_request,
            IConfiguration config) =>
            CreateFacebookDeletionResponse(ctx, signed_request, config))
            .AllowAnonymous();

        return app;
    }

    private static async Task StartFacebookLoginAsync(
        SignInManager<ApplicationUser> signInManager,
        HttpContext ctx)
    {
        var props = signInManager
            .ConfigureExternalAuthenticationProperties("Facebook", "/finished");
        await ctx.ChallengeAsync("Facebook", props);
    }

    private static async Task<IResult> FinishFacebookLoginAsync(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IPlayerDbService playerDb,
        IAuditLogger audit,
        HttpContext ctx)
    {
        var qs = ctx.Request.QueryString.Value;
        var uriReturn = "/checkin";
        if (!string.IsNullOrEmpty(qs) && qs.Contains("error=", StringComparison.OrdinalIgnoreCase))
        {
            await ctx.SignOutAsync(IdentityConstants.ExternalScheme);
            return Results.Redirect("/" + qs);
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            await WriteAuditAsync(audit, ctx, AuditEvents.LOGIN, AuditOutcome.Failed, null, null);
            return Results.Redirect("/?error=NoInfo");
        }

        var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (user == null)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        ?? $"fb_{info.ProviderKey}@example.com";
            user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = await CreateFacebookUserAsync(email, userManager, audit, ctx);
                if (user == null)
                {
                    await WriteAuditAsync(audit, ctx, AuditEvents.LOGIN, AuditOutcome.Failed, null, null);
                    return Results.Redirect("/?error=CreateFailed");
                }

                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var suggestedName = await playerDb.SuggestAsync(firstName);
                uriReturn = $"/checkin?name={Uri.EscapeDataString(suggestedName)}";
            }

            if (!await LinkFacebookLoginAsync(user, info, userManager, audit, ctx))
            {
                await WriteAuditAsync(audit, ctx, AuditEvents.LOGIN, AuditOutcome.Failed, null, user.Id);
                return Results.Redirect("/?error=LinkFailed");
            }
        }

        await ConfirmMatchingFacebookEmailAsync(user, info, userManager);
        await signInManager.SignInAsync(user, isPersistent: false);
        await signInManager.UpdateExternalAuthenticationTokensAsync(info);
        await ctx.SignOutAsync(IdentityConstants.ExternalScheme);
        await WriteAuditAsync(audit, ctx, AuditEvents.LOGIN, AuditOutcome.Succeeded, user.Id, user.Id);
        return Results.Redirect(uriReturn);
    }

    private static async Task<ApplicationUser?> CreateFacebookUserAsync(
        string email,
        UserManager<ApplicationUser> userManager,
        IAuditLogger audit,
        HttpContext ctx)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            // A külső szolgáltató által átadott e-mail-cím jelenleg megerősítettnek minősül.
            // A bizalmi szint megváltoztatása regisztrációs és fiók-összekapcsolási döntés is.
            EmailConfirmed = true
        };
        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            await WriteAuditAsync(audit, ctx, AuditEvents.ACCOUNT_REGISTERED, AuditOutcome.Failed, null, null);
            return null;
        }

        await WriteAuditAsync(audit, ctx, AuditEvents.ACCOUNT_REGISTERED, AuditOutcome.Succeeded, user.Id, user.Id);
        return user;
    }

    private static async Task<bool> LinkFacebookLoginAsync(
        ApplicationUser user,
        ExternalLoginInfo info,
        UserManager<ApplicationUser> userManager,
        IAuditLogger audit,
        HttpContext ctx)
    {
        var linkResult = await userManager.AddLoginAsync(user, info);
        if (!linkResult.Succeeded)
        {
            await WriteAuditAsync(audit, ctx, AuditEvents.EXTERNAL_LOGIN_LINKED, AuditOutcome.Failed, null, user.Id);
            return false;
        }

        await WriteAuditAsync(audit, ctx, AuditEvents.EXTERNAL_LOGIN_LINKED, AuditOutcome.Succeeded, user.Id, user.Id);
        return true;
    }

    private static async Task ConfirmMatchingFacebookEmailAsync(
        ApplicationUser user,
        ExternalLoginInfo info,
        UserManager<ApplicationUser> userManager)
    {
        if (user.EmailConfirmed)
            return;

        var claimEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(claimEmail) ||
            !string.Equals(claimEmail, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await userManager.ConfirmEmailAsync(user, code);
    }

    private static async Task<IResult> DeauthorizeFacebookAsync(
        [FromForm] string signed_request,
        IConfiguration config,
        UserManager<ApplicationUser> userManager,
        IAuditLogger audit,
        HttpContext ctx)
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

            await WriteAuditAsync(
                audit,
                ctx,
                AuditEvents.EXTERNAL_LOGIN_REMOVED,
                removeResult.Succeeded ? AuditOutcome.Succeeded : AuditOutcome.Failed,
                null,
                user.Id);
        }

        return Results.Ok(new { status = "ok" });
    }

    private static IResult CreateFacebookDeletionResponse(
        HttpContext ctx,
        [FromForm] string signed_request,
        IConfiguration config)
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
    }

    private static Task WriteAuditAsync(
        IAuditLogger audit,
        HttpContext ctx,
        string eventName,
        AuditOutcome outcome,
        string? actorId,
        string? subjectId)
    {
        return audit.LogAsync(
            new AuditEntry(
                eventName,
                outcome,
                actorId,
                subjectId,
                ctx.Connection.RemoteIpAddress?.ToString(),
                ctx.TraceIdentifier));
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
