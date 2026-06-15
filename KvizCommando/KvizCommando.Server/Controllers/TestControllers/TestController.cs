using KvizCommando.Server.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KvizCommando.Server.Controllers.TestControllers
{
    [ApiController]
    [Route("api/game/[controller]")]
    //[Authorize] // minden action-hoz kell token
   // [Authorize(Policy = "Api")]
    [Authorize(Policy = TermsAcceptedRequirement.PolicyName)]
    public class TestController : ControllerBase
    {
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
