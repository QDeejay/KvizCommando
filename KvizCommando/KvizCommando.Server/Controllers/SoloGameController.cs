using KvizCommando.Server.Authorization;
using KvizCommando.Server.Extensions;
using KvizCommando.Server.Services.SoloGame;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace KvizCommando.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = TermsAcceptedRequirement.PolicyName)]
    public class SoloGameController : ControllerBase
    {
        private readonly ILogger<SoloGameController> _logger;
        private readonly ISoloGameService _soloGameService;
        private readonly IStringLocalizer<SoloGameController> _localizer;
        private readonly IUserPlayerIdCacheService _idCache;

        public SoloGameController(
            ILogger<SoloGameController> logger,
            ISoloGameService soloGameService,
            IStringLocalizer<SoloGameController> localizer,
            IUserPlayerIdCacheService userPlayerId)
        {
            _logger = logger;
            _soloGameService = soloGameService;
            _localizer = localizer;
            _idCache = userPlayerId;
        }

        [HttpPost("start")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(StartSoloGameResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        [ProducesResponseType(501)]
        public async Task<ActionResult<StartSoloGameResponse>> StartSoloGameAsync(
            [FromBody] StartSoloGameRequest dto,
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

            if (userId == null)
                return Unauthorized();

            if (dto == null || string.IsNullOrWhiteSpace(dto.SessionId) || dto.SelectionId < 1)
            {
                Response.AddToast(_localizer["SoloGame.Error.InvalidData"].Value, ToastType.Error);
                return BadRequest();
            }

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var result = await _soloGameService.StartAsync(playerId.Value, dto, ct);

            if (result.Success == null)
            {
                _logger.LogWarning("Session ID probléma user:{UserId} sessionId:{SessionId}", userId, dto.SessionId);
                Response.AddToast(_localizer["SoloGame.Error.Session"].Value, ToastType.Error);
                return StatusCode(501);
            }
            else if (result.Success == false || result.Response == null)
            {
                _logger.LogWarning("Solo játék indítása sikertelen. userId={UserId}", userId);
                Response.AddToast(_localizer["SoloGame.Error.ActiveGame"].Value, ToastType.Warning);
                return Conflict();
            }
            else
            {
                Response.AddToast(_localizer["SoloGame.Response.Started"].Value, ToastType.Info);
                return Ok(result.Response);
            }
        }

        [HttpPost("{gameId:guid}/finish")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(FinishSoloGameResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(501)]
        public async Task<ActionResult<FinishSoloGameResponse>> FinishSoloGameAsync(
            [FromRoute] Guid gameId,
            [FromBody] FinishSoloGameRequest dto,
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

            if (userId == null)
                return Unauthorized();

            if (dto == null || string.IsNullOrWhiteSpace(dto.SessionId) || dto.Answers == null)
            {
                Response.AddToast(_localizer["SoloGame.Error.InvalidData"].Value, ToastType.Error);
                return BadRequest();
            }

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var result = await _soloGameService.FinishAsync(playerId.Value, gameId, dto, ct);

            if (result.Success == null)
            {
                _logger.LogWarning("Session ID probléma user:{UserId} sessionId:{SessionId}", userId, dto.SessionId);
                Response.AddToast(_localizer["SoloGame.Error.Session"].Value, ToastType.Error);
                return StatusCode(501);
            }
            else if (result.Success == false || result.Response == null)
            {
                _logger.LogWarning("Solo játék lezárása sikertelen. userId={UserId} gameId={GameId}", userId, gameId);
                Response.AddToast(_localizer["SoloGame.Error.InvalidFinish"].Value, ToastType.Error);
                return BadRequest();
            }
            else
            {
                Response.AddToast(_localizer["SoloGame.Response.Finished"].Value, ToastType.Success);
                return Ok(result.Response);
            }
        }

        [HttpPost("{gameId:guid}/abandon")]
        [Consumes("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(501)]
        public async Task<IActionResult> AbandonSoloGameAsync(
            [FromRoute] Guid gameId,
            [FromBody] AbandonSoloGameRequest dto,
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

            if (userId == null)
                return Unauthorized();

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var success = await _soloGameService.AbandonAsync(playerId.Value, gameId, dto.SessionId, ct);

            if (success == null)
            {
                _logger.LogWarning("Session ID probléma user:{UserId} sessionId:{SessionId}", userId, dto.SessionId);
                Response.AddToast(_localizer["SoloGame.Error.Session"].Value, ToastType.Error);
                return StatusCode(501);
            }
            else if (success == false)
            {
                _logger.LogWarning("Solo játék elhagyása sikertelen. userId={UserId} gameId={GameId}", userId, gameId);
                Response.AddToast(_localizer["SoloGame.Error.NotFound"].Value, ToastType.Error);
                return NotFound();
            }
            else
            {
                Response.AddToast(_localizer["SoloGame.Response.Abandoned"].Value, ToastType.Warning);
                return NoContent();
            }
        }
    }
}
