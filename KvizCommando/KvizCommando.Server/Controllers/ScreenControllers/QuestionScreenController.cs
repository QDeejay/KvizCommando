
/*using KvizCommando.Server.Authorization;
using KvizCommando.Server.Services.DtoMapping;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KvizCommando.Server.Controllers.ScreenControllers
{
    [ApiController]
    [Route("api/questions")]
    [Authorize(Policy = TermsAcceptedRequirement.PolicyName)]
    public class QuestionScreenController : ControllerBase
    {
        private readonly IScreenService _screenService;
        private readonly IUserPlayerIdCacheService _idCache;

        public QuestionScreenController(IScreenService screenService, IUserPlayerIdCacheService userPlayerId)
        {
            _screenService = screenService;
            _idCache = userPlayerId;
        }

        /// <summary>Kérdés képernyő komponált DTO.</summary>
        [HttpGet] // GET /api/questions
        [ProducesResponseType(typeof(QuestionDtos), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<QuestionDtos>> GetQuestionScreenAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

            if (userId == null)
                return Unauthorized();
            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var dto = await _questionScreenService.GetQuestionScreenAsync(playerId.Value, ct);
            if (dto is null)
                return NotFound();

            return Ok(dto);
        }
    }
}*/