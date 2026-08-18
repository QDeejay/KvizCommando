using KvizCommando.Server.Authorization;
using KvizCommando.Server.Infrastructure.Http;
using KvizCommando.Server.Services.DtoMapping;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
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
    public class TeamController : ControllerBase
    {
        private readonly ILogger<TeamController> _logger;
        private readonly ITeamService _teamService;
        private readonly IStringLocalizer<TeamController> _localizer;
        private readonly IUserPlayerIdCacheService _idCache;

        public TeamController(
            ILogger<TeamController> logger,
            ITeamService teamservice,
            IStringLocalizer<TeamController> localizer,
            IUserPlayerIdCacheService userPlayerId)
        {
            _logger = logger;
            _teamService = teamservice;
            _localizer = localizer;
            _idCache = userPlayerId;
        }


        /// <summary>
        /// Lekéri a csapatképernyő megjelenítési adatait.
        /// </summary>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        [HttpGet("screen")]
        [ProducesResponseType(typeof(TeamDtos), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TeamDtos>> GetTeamScreenAsync([FromQuery] string sessionId, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");
            if (userId == null)
                return Unauthorized();
            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var dto = await _teamService.GetTeamScreenDataAsync(playerId.Value, sessionId, ct);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }



        /// <summary>
        /// Feldolgozza a karakter képességpontjainak mentési kérését.
        /// </summary>
        /// <param name="dto">A feldolgozandó kérés adatai.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        [HttpPost("modify")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        public async Task<ActionResult<ApiResponse>> SaveSkillsAsync(
           [FromBody] ModifySkillRequest dto,
           CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");
            if (userId == null)
                return Unauthorized();

            if (dto.SkillType > 2 || dto.SkillType < 1 || dto.MemberId > 8)
                return FailToast(400, _localizer["Resp.Error.InValidData"].Value);

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);

            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");


            var result = await _teamService.SaveModifiedSkillAsync(playerId.Value, dto, ct);

            if (result == CacheUpdateResult.SessionMismatch)
            {
                _logger.LogWarning($"Session ID probléma user:{userId} sessionId:", dto.SessionId);
                return FailToast(409, _localizer["Error.Session"].Value);
            }

            if (result == CacheUpdateResult.NotFound)
                return NotFound(ApiResponse.Fail());

            if (result == CacheUpdateResult.Rejected)
            {
                _logger.LogWarning($"Skill modosítás sikertelen. userId={userId}", userId);
                return FailToast(400, _localizer["Error.Internal"].Value);
            }

            return OkToast(_localizer["Resp.SaveSkill", dto.SkillChanges.Sum()].Value, ToastType.Success);
        }

        [HttpPost("manage")] // POST /api/team/manage
        /// <summary>
        /// Végrehajtja a csapaton kért kezelési műveletet.
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
        public async Task<ActionResult<ApiResponse>> ManageTeamAsync(
           [FromBody] ManageTeamRequest dto,
           CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub")
                    ?? throw new InvalidOperationException("Missing user id");
            if (userId == null)
                return Unauthorized();

            if ((int)dto.ReqType > 4 || (int)dto.ReqType < 0)
                return FailToast(400, _localizer["Resp.Error.InvalidRequest"].Value);


            if (dto.MemberNo < 1 || dto.MemberNo > 8)
                return FailToast(400, _localizer["Resp.Error.InvalidMember"].Value);

            if ((int)dto.ReqType == 0 && (dto.CandidateId < 1 || dto.CandidateId > 8))
                return FailToast(400, _localizer["Resp.Error.InvalidCandidate"].Value);

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");


            var result = await _teamService.ManageTeamAsync(playerId.Value, dto, ct);


            if (result == CacheUpdateResult.SessionMismatch)
            {
                _logger.LogWarning($"Session ID probléma user:{userId} sessionId:", dto.SessionId);
                return FailToast(409, _localizer["Error.Session"].Value);
            }

            if (result == CacheUpdateResult.NotFound)
                return NotFound(ApiResponse.Fail());

            if (result == CacheUpdateResult.Rejected)
            {
                _logger.LogWarning($"Csapatmodositás sikertelen ({dto.ReqType.ToString()}) sikertelen. userId={userId}", userId);
                return FailToast(400, _localizer["Error.Internal"].Value);
            }

            var respToast = dto.ReqType switch
            {
                ManageType.Fire => ToastType.Warning,
                ManageType.Heal => ToastType.Info,
                ManageType.Hire => ToastType.Success,
                ManageType.Promote => ToastType.Info,
                ManageType.Retire => ToastType.Warning,
                _ => ToastType.Error,
            };
            return OkToast(_localizer[$"Resp.{dto.ReqType}"].Value, respToast);

        }

        /// <summary>
        /// A csapatműveletek egységes sikerességi válaszát írja le.
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
