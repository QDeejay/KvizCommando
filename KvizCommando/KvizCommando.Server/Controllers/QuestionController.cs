using KvizCommando.Server.Authorization;
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Services.DtoMapping;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.SqlServer.Server;
using System.Security.Claims;

namespace KvizCommando.Server.Controllers
{
    /// <summary>
    /// Domain műveletek a Question oldalhoz (POST/PUT/DELETE).
    /// Jelenleg: factory slotok mentése.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = TermsAcceptedRequirement.PolicyName)]
    public class QuestionController : ControllerBase
    {
        private readonly ILogger<QuestionController> _logger;
        private readonly IQuestionService _questionService;
        private readonly IStringLocalizer<QuestionController> _localizer;
        private readonly IUserPlayerIdCacheService _idCache;

        public QuestionController(
            ILogger<QuestionController> logger,
            IQuestionService questionservice,
            IStringLocalizer<QuestionController> localizer,
            IUserPlayerIdCacheService userPlayerId)
        {
            _logger = logger;
            _questionService = questionservice;
            _localizer = localizer;
            _idCache = userPlayerId;
        }

        /// <summary>Factory slotok mentése az aktuális játékoshoz.</summary>
        /// <remarks>Elvárt body: <see cref="UpdateFactorySlotsDto"/>.</remarks>
        [HttpPost("factory")] // POST /api/questions/factory
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse>> SaveFactoryAsync(
            [FromBody] SaveFactoryRequest dto,
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

            if (userId == null)
                return Unauthorized();

            // alap validáció
            if (dto?.CategorySlots is null || dto.CategorySlots.Length == 0)
                return BadRequest(ApiResponse.Fail(_localizer["Error.MustOneSlot"].Value));

            if (dto.CategorySlots.Any(x => x < 0))
                return BadRequest(ApiResponse.Fail(_localizer["Error.InValidData"].Value));


            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");


            // write-through frissítés a cache-ben + store-ban
            var success = await _questionService.SaveFactorySlotsAsync(playerId.Value, dto,ct);

            if (!success)
            {
                _logger.LogWarning("Factory slot mentés sikertelen. userId={UserId}", userId);
                return StatusCode(500, ApiResponse.Fail(_localizer["Error.Internal"].Value));
            }

            return Ok(ApiResponse.Ok(_localizer["Resp.SaveOk"].Value));
        }

        [HttpPost("manageslot")] // POST /api/questions/manageslot
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse>> ManageSlotAsync(
            [FromBody] ManageSlotRequest dto,
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

            if (userId == null)
                return Unauthorized();

            if (dto == null || dto.ReqType.ToString()=="")
                return BadRequest(ApiResponse.Fail(_localizer["Error.InValidData"].Value));

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var success = await _questionService.ManageSlotsAsync(playerId.Value, dto, ct);
            if (!success)
            {
                _logger.LogWarning("Factory slot mentés sikertelen. userId={UserId}", userId);
                return StatusCode(500, ApiResponse.Fail(_localizer["Error.Internal"].Value));
            }
            return Ok(ApiResponse.Ok(_localizer["Resp.SaveOk"].Value));
        }

        [HttpPost("sendnew")] // POST /api/questions/sendnew
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse>> NewQuestionAsync(
           [FromBody] NewQuestionRequest dto,
           CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

            if (userId == null)
                return Unauthorized();

            if (dto==null || dto.Category==0 || !dto.Question.Contains('?') || dto.Question.Length<10 || dto.Question.Length >200)
                return BadRequest(ApiResponse.Fail(_localizer["Resp.Qustion.BadData"].Value));

            if ( dto.Question.Length < 10 || dto.Question.Length > 200)
                return BadRequest(ApiResponse.Fail(_localizer["Resp.Question.TooLong"].Value));

            if (dto.Answers.Any(a => string.IsNullOrWhiteSpace(a)))
                return BadRequest(ApiResponse.Fail(_localizer["Resp.Answer.BadData"].Value));

            if (dto.Answers.Distinct().Count() != dto.Answers.Length)
                return BadRequest(ApiResponse.Fail(_localizer["Resp.Answer.Notdifferent"].Value));

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var success = await _questionService.SendNewQuestionAsync(playerId.Value, dto, ct);
            if (!success)
            {
                _logger.LogWarning($"Új kérdés mentés sikertelen. userId={userId}", userId);
                return StatusCode(500, ApiResponse.Fail(_localizer["Error.Internal"].Value));
            }


            return Ok(ApiResponse.Ok(_localizer["Resp.SaveOk"].Value));
        }

        // Egységes API válasz
        public sealed record ApiResponse(bool Success, string Message, string? ServerVersion = null)
        {
            public static ApiResponse Ok(string msg) => new(true, msg);
            public static ApiResponse Fail(string msg) => new(false, msg);
        }
    }
}


