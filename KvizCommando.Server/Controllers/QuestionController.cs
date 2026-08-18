using KvizCommando.Server.Authorization;
using KvizCommando.Server.Extensions;
using KvizCommando.Server.Services.DtoMapping;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums;
using KvizCommando.Shared.Models.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace KvizCommando.Server.Controllers
{
    /// <summary>
    /// A kérdésképernyő lekérdezési, kérdéshely-kezelési és újkérdés-beküldési végpontjait biztosítja.
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

        /// <summary>
        /// Lekéri a kérdéskezelő képernyő megjelenítési adatait.
        /// </summary>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        [HttpGet("screen")]
        [ProducesResponseType(typeof(QuestionDtos), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<QuestionDtos>> GetQuestionScreenAsync([FromQuery] string sessionId, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

            if (userId == null)
                return Unauthorized();

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);

            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var dto = await _questionService.GetQuestionScreenAsync(playerId.Value, sessionId, ct);
            if (dto is null)
                return NotFound();

            return Ok(dto);
        }




        /// <summary>
        /// Elmenti az aktuális játékos gyári kérdéshelyeinek összeállítását.
        /// </summary>
        /// <param name="dto">A feldolgozandó kérés adatai.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        [HttpPost("factory")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        public async Task<ActionResult<ApiResponse>> SaveFactoryAsync(
            [FromBody] SaveFactoryRequest dto,
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

            if (userId == null)
                return Unauthorized();

            if (dto?.CategorySlots is null || dto.CategorySlots.Length == 0)
                return FailToast(400, _localizer["Error.MustOneSlot"].Value);

            if (dto.CategorySlots.Any(x => x < 0))
                return FailToast(400, _localizer["Error.InValidData"].Value);


            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");


            // A módosítás ugyanazon műveletben frissíti a cache-t és jelöli tartós mentésre az érintett adatot.
            var result = await _questionService.SaveFactorySlotsAsync(playerId.Value, dto, ct);

            if (result == CacheUpdateResult.SessionMismatch)
            {
                _logger.LogWarning($"Session ID probléma user:{userId} sessionId:", dto.SessionId);
                return FailToast(409, _localizer["Error.Session"].Value);
            }

            if (result == CacheUpdateResult.NotFound)
                return NotFound(ApiResponse.Fail());

            if (result == CacheUpdateResult.Rejected)
            {
                _logger.LogWarning("Érvénytelen factory loadout mentés. userId={UserId}", userId);
                return FailToast(400, _localizer["Error.InValidData"].Value);
            }

            return OkToast(_localizer["Resp.SaveOk"].Value, ToastType.Success);




        }

        [HttpPost("manageslot")] // POST /api/questions/manageslot
        /// <summary>
        /// Végrehajtja a kérdéshelyen kért kezelési műveletet.
        /// </summary>
        /// <param name="dto">A feldolgozandó kérés adatai.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        public async Task<ActionResult<ApiResponse>> ManageSlotAsync(
            [FromBody] ManageSlotRequest dto,
            CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

            if (userId == null)
                return Unauthorized();

            if (dto == null || dto.ReqType.ToString() == "")
                return FailToast(400, _localizer["Error.InValidData"].Value);

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var result = await _questionService.ManageSlotsAsync(playerId.Value, dto, ct);
            string action = dto.ReqType switch
            {
                SlotManageType.DeleteUsr => "DeleteOk",
                SlotManageType.DeletePending => "DeleteOk",
                SlotManageType.MovePending => "MoveOk",
                _ => "SaveOk"
            };
            if (result == CacheUpdateResult.SessionMismatch)
            {
                _logger.LogWarning($"Session ID probléma user:{userId} sessionId:", dto.SessionId);
                return FailToast(409, _localizer["Error.Session"].Value);
            }

            if (result == CacheUpdateResult.NotFound)
                return NotFound(ApiResponse.Fail());

            if (result == CacheUpdateResult.Rejected)
            {
                _logger.LogWarning($"Slot művelet: ({dto.ReqType.ToString()}) sikertelen. userId={userId}", userId);
                return FailToast(400, _localizer["Error.Internal"].Value);
            }

            return OkToast(_localizer[$"Resp.{action}"].Value, action == "MoveOk" ? ToastType.Info : ToastType.Warning);


        }

        [HttpPost("sendnew")] // POST /api/questions/sendnew
        /// <summary>
        /// Beküldi az új felhasználói kérdést ellenőrzésre.
        /// </summary>
        /// <param name="dto">A feldolgozandó kérés adatai.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        public async Task<ActionResult<ApiResponse>> NewQuestionAsync(
           [FromBody] NewQuestionRequest dto,
           CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

            if (userId == null)
                return Unauthorized();

            if (dto == null ||
                dto.Category is < 1 or > 16 ||
                string.IsNullOrWhiteSpace(dto.Question) ||
                !dto.Question.Contains('?'))
                return FailToast(400, _localizer["Resp.Qustion.BadData"].Value);

            if (dto.Question.Length < NewQuestionRules.QUESTION_MIN_LENGTH ||
                dto.Question.Length > NewQuestionRules.QUESTION_MAX_LENGTH)
            {
                return FailToast(
                    400,
                    _localizer[
                        "Resp.Question.TooLong",
                        NewQuestionRules.QUESTION_MIN_LENGTH,
                        NewQuestionRules.QUESTION_MAX_LENGTH].Value);
            }

            if (dto.Answers is not { Length: 4 } ||
                dto.Answers.Any(answer =>
                    string.IsNullOrWhiteSpace(answer) ||
                    answer.Length > NewQuestionRules.ANSWER_MAX_LENGTH))
                return FailToast(400, _localizer["Resp.Answer.BadData"].Value);

            if (dto.Answers.Distinct().Count() != dto.Answers.Length)
                return FailToast(400, _localizer["Resp.Answer.Notdifferent"].Value);

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var result = await _questionService.SendNewQuestionAsync(playerId.Value, dto, ct);

            if (result == CacheUpdateResult.SessionMismatch)
            {
                _logger.LogWarning($"Session ID probléma user:{userId} sessionId:", dto.SessionId);
                return FailToast(409, _localizer["Error.Session"].Value);
            }

            if (result == CacheUpdateResult.NotFound)
                return NotFound(ApiResponse.Fail());

            if (result == CacheUpdateResult.Rejected)
            {
                _logger.LogWarning($"Új kérdés mentés sikertelen. userId={userId}", userId);
                return FailToast(400, _localizer["Error.Internal"].Value);
            }

            return OkToast(_localizer["Resp.SendOk"].Value, ToastType.Info);



        }

        /// <summary>
        /// A kérdésműveletek egységes sikerességi válaszát írja le.
        /// </summary>
        /// <param name="Success">Jelzi, hogy az üzleti művelet sikeresen befejeződött-e.</param>
        /// <param name="ServerVersion">A válaszhoz tartozó opcionális szerververzió.</param>
        public sealed record ApiResponse(bool Success, string? ServerVersion = null)
        {
            /// <summary>
            /// Sikeres API-választ hoz létre.
            /// </summary>
            public static ApiResponse Ok() => new(true);
            /// <summary>
            /// Sikertelen API-választ hoz létre.
            /// </summary>
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
