using KvizCommando.Server.Authorization;
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Services.DtoMapping;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;
using System.Security.Claims;
using KvizCommando.Server.Extensions;

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
        [ProducesResponseType(typeof(ApiResponse), 501)]
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
                return FailToast(400, _localizer["Error.MustOneSlot"].Value);

            if (dto.CategorySlots.Any(x => x < 0))
                return FailToast(400, _localizer["Error.InValidData"].Value);


            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

          
            // write-through frissítés a cache-ben + store-ban
            var success = await _questionService.SaveFactorySlotsAsync(playerId.Value, dto,ct);

            if (success == null)
            {
                _logger.LogWarning($"Session ID probléma user:{userId} sessionId:", dto.SessionId);
                return FailToast(501, _localizer["Error.Session"].Value);
            }
            else if (success == false)
            {
                _logger.LogWarning("Factory slot mentés sikertelen. userId={UserId}", userId);
                return FailToast(500, _localizer["Error.Internal"].Value);
            }
            else
                return OkToast(_localizer["Resp.SaveOk"].Value, ToastType.Success);
            
                


        }

        [HttpPost("manageslot")] // POST /api/questions/manageslot
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        [ProducesResponseType(typeof(ApiResponse), 501)]
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
                return FailToast(400, _localizer["Error.InValidData"].Value);

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var success = await _questionService.ManageSlotsAsync(playerId.Value, dto, ct);
            string action = dto.ReqType switch
            {
                SlotManageType.DeleteUsr => "DeleteOk",
                SlotManageType.DeletePending => "DeleteOk",
                SlotManageType.MovePending => "MoveOk",
                _ => "SaveOk"
            };
            if (success == null)
            {
                _logger.LogWarning($"Session ID probléma user:{userId} sessionId:", dto.SessionId);
                return FailToast(501, _localizer["Error.Session"].Value);
            }
            else if (success == false)
            {
                _logger.LogWarning($"Slot művelet: ({dto.ReqType.ToString()}) sikertelen. userId={userId}", userId);
                return FailToast(500, _localizer["Error.Internal"].Value);
            }
            else
                return OkToast(_localizer[$"Resp.{action}"].Value, action=="MoveOk" ? ToastType.Info : ToastType.Warning);


        }

        [HttpPost("sendnew")] // POST /api/questions/sendnew
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        [ProducesResponseType(typeof(ApiResponse), 501)]
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
                return FailToast(400, _localizer["Resp.Qustion.BadData"].Value);

            if ( dto.Question.Length < 10 || dto.Question.Length > 200)
                return FailToast(400, _localizer["Resp.Question.TooLong"].Value);

            if (dto.Answers.Any(a => string.IsNullOrWhiteSpace(a)))
                return FailToast(400, _localizer["Resp.Answer.BadData"].Value);

            if (dto.Answers.Distinct().Count() != dto.Answers.Length)
                return FailToast(400, _localizer["Resp.Answer.Notdifferent"].Value);

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var success = await _questionService.SendNewQuestionAsync(playerId.Value, dto, ct);
           
            if (success == null)
            {
                _logger.LogWarning($"Session ID probléma user:{userId} sessionId:", dto.SessionId);
                return FailToast(501, _localizer["Error.Session"].Value);
            }
            else if (success == false)
            {
                _logger.LogWarning($"Új kérdés mentés sikertelen. userId={userId}", userId);
                return FailToast(500, _localizer["Error.Internal"].Value);
            }
            else
                return OkToast(_localizer["Resp.SendOk"].Value, ToastType.Info);

        }

        // Egységes API válasz
        public sealed record ApiResponse(bool Success, string? ServerVersion = null)
        {
            public static ApiResponse Ok() => new(true);
            public static ApiResponse Fail() => new(false);
        }

        private ActionResult<ApiResponse> OkToast(string text, ToastType type)
        {
            Response.AddToast(text, type);
            return Ok(ApiResponse.Ok());
        }

        private ActionResult<ApiResponse> FailToast(int statusCode, string text)
        {
            Response.AddToast(text, ToastType.Error);
            return StatusCode(statusCode, ApiResponse.Fail());
        }
    }
   
}


