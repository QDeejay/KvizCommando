using KvizCommando.Server.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KvizCommando.Server.Controllers.TestControllers
{
    [ApiController]
    [Route("api/game/[controller]")]
    [Authorize(Policy = TermsAcceptedRequirement.POLICY_NAME)]
    // Szándékosan megmaradó próba-végpont a mobil és asztali kliensek
    // cookie/bearer hitelesítésének gyors ellenőrzéséhez.
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Visszaadja a hitelesített tesztfelhasználó azonosítóját.
        /// </summary>
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            return Ok(new { userId });
        }
    }
}
