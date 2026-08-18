using KvizCommando.Server.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace KvizCommando.Server.Controllers;

[ApiController]
[Route("api/account")]
[Authorize(Policy = "Api")]
public sealed class AccountLinkController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountLinkController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Leválasztja a Facebook-bejelentkezést az aktuális felhasználóról.
    /// </summary>
    [HttpPost("unlink/facebook")]
    public async Task<IActionResult> UnlinkFacebookAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var logins = await _userManager.GetLoginsAsync(user);
        var fb = logins.FirstOrDefault(l => l.LoginProvider == "Facebook");
        if (fb is null) return NotFound(new { error = "no_facebook_link" });

        var rmLogin = await _userManager.RemoveLoginAsync(user, fb.LoginProvider, fb.ProviderKey);
        if (!rmLogin.Succeeded)
            return Problem("remove_login_failed", statusCode: 500);

        // A szolgáltatói tokenek a leválasztott bejelentkezéssel együtt érvényüket vesztik.
        await _userManager.RemoveAuthenticationTokenAsync(user, "Facebook", "access_token");
        await _userManager.RemoveAuthenticationTokenAsync(user, "Facebook", "expires_at");
        await _userManager.RemoveAuthenticationTokenAsync(user, "Facebook", "token_type");

        return Ok(new { status = "ok" });
    }
}
