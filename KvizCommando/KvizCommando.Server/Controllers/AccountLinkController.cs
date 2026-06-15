using KvizCommando.Server.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace KvizCommando.Server.Controllers;

[ApiController]
[Route("api/account")]
[Authorize(Policy = "Api")]  // policy-séma szerint
public sealed class AccountLinkController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountLinkController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // 3) Játékból indított unlink — aktuális felhasználótól leválasztja a Facebookot
    [HttpPost("unlink/facebook")]
    public async Task<IActionResult> UnlinkFacebookAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var logins = await _userManager.GetLoginsAsync(user);
        var fb = logins.FirstOrDefault(l => l.LoginProvider == "Facebook");
        if (fb is null) return NotFound(new { error = "no_facebook_link" });

        // Login leválasztása
        var rmLogin = await _userManager.RemoveLoginAsync(user, fb.LoginProvider, fb.ProviderKey);
        if (!rmLogin.Succeeded)
            return Problem("remove_login_failed", statusCode: 500);

        // Tokenek törlése (gyári kulcsok)
        await _userManager.RemoveAuthenticationTokenAsync(user, "Facebook", "access_token");
        await _userManager.RemoveAuthenticationTokenAsync(user, "Facebook", "expires_at");
        await _userManager.RemoveAuthenticationTokenAsync(user, "Facebook", "token_type");

        // (Opcionális) ha ez volt az utolsó belépési mód és nincs jelszó, itt jelezhetsz a kliensnek

        return Ok(new { status = "ok" });
    }
}
