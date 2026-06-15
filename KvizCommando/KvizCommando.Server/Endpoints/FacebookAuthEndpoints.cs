using KvizCommando.Client;
using KvizCommando.Server.Identity;
using KvizCommando.Server.Services.CheckIn;
using KvizCommando.Server.Services.Db;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace KvizCommando.Server.Endpoints;

public static class FacebookAuthEndpoints
{
    public static IEndpointRouteBuilder MapFacebookAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // 1) Login indítás → Facebook redirect
        app.MapGet("/login/facebook", async (
             SignInManager<ApplicationUser> signInManager,
             HttpContext ctx) =>
        {
            var props = signInManager
                .ConfigureExternalAuthenticationProperties("Facebook", "/finished");
            await ctx.ChallengeAsync("Facebook", props);
        });
        // 2) Finish → gyári info + token perzisztálás AspNetUserTokens táblába
        // 2) Callback – mind a 4 fő eset korrekt lekezelése + token persist
        app.MapGet("/finished", async (
                SignInManager<ApplicationUser> signInManager,
                UserManager<ApplicationUser> userManager,
                IPlayerDbService playerDb,
                HttpContext ctx) =>
        {
            var qs = ctx.Request.QueryString.Value;
            var uriReturn = $"/checkin?name={Uri.EscapeDataString("OK")}";
            if (!string.IsNullOrEmpty(qs) && qs.Contains("error=", StringComparison.OrdinalIgnoreCase))
            {
                await ctx.SignOutAsync(IdentityConstants.ExternalScheme); // takarítás
                return Results.Redirect("/" + qs); // egy az egyben továbbadjuk
            }


            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return Results.Redirect("/?error=NoInfo");

            // (A) Már van ilyen külső login → user megvan
            var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            // (B) Ha nincs, próbáljuk e-mail alapján (linkelés vagy új user)
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
                        // Dev-barát: külső login esetén megerősítettnek tekintjük (ha prod-ban nem akarod, vedd ki)
                        EmailConfirmed = true
                    };
                    var cr = await userManager.CreateAsync(user);
                    if (!cr.Succeeded)
                        return Results.Redirect("/?error=CreateFailed");
                    var FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                    var SuggestedName = await playerDb.SuggestAsync(FirstName);
                    uriReturn = $"/checkin?name={Uri.EscapeDataString(SuggestedName)}";
                }

                var lr = await userManager.AddLoginAsync(user, info);
                if (!lr.Succeeded)
                    return Results.Redirect("/?error=LinkFailed");
            }

            // (opcionális, de rövid és hasznos): ha Confirmed kötelező és az e-mail egyezik, erősítsük meg
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
            await signInManager.UpdateExternalAuthenticationTokensAsync(info); // access_token, expires_at → AspNetUserTokens
            await ctx.SignOutAsync(IdentityConstants.ExternalScheme);          // external cookie takarítás
           


            return Results.Redirect(uriReturn);
        });
        // 3) Facebook DEAUTHORIZE callback — user app-eltávolítás: link + token törlés
        // FB: POST form 'signed_request'; válasz: 200 OK elég. (Meta: Deauthorize)
        app.MapPost("/facebook/deauthorize", async (
            [FromForm] string signed_request,
            IConfiguration config,
            UserManager<ApplicationUser> userManager) =>
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
                await userManager.RemoveLoginAsync(user, "Facebook", fbUserId);
                await userManager.RemoveAuthenticationTokenAsync(user, "Facebook", "access_token");
                await userManager.RemoveAuthenticationTokenAsync(user, "Facebook", "expires_at");
                await userManager.RemoveAuthenticationTokenAsync(user, "Facebook", "token_type");
            }
            return Results.Ok(new { status = "ok" });
        }).AllowAnonymous();

        // 4) Facebook DATA DELETION callback — FB elvárása: { url, confirmation_code } JSON
        // (Meta: Data Deletion Callback)
        app.MapPost("/facebook/deletion", async (
            HttpContext ctx,
            [FromForm] string signed_request,
            IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(signed_request))
                return Results.BadRequest(new { error = "missing_signed_request" });

            var appSecret = config["Authentication:Facebook:AppSecret"];
            if (string.IsNullOrWhiteSpace(appSecret))
                return Results.BadRequest(new { error = "missing_app_secret" });

            if (!TryVerifyAndDecodeSignedRequest(signed_request, appSecret, out var _))
                return Results.BadRequest(new { error = "invalid_signed_request" });

            var code = Guid.NewGuid().ToString("N");
            var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
            // opcionális státusz oldalad (GDPR log/azonosító)
            var statusUrl = $"{baseUrl}/privacy/data-deletion?code={code}";
            await Task.Delay(1); // szimuláljunk valami aszinkront
            return Results.Ok(new { url = statusUrl, confirmation_code = code });
        }).AllowAnonymous();

        return app;
    }

    // --- Helpers (Facebook 'signed_request' HMAC-SHA256 + base64url) ---

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
            payload = doc.RootElement.Clone(); // detach az eldobás előtt
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




