using KvizCommando.Server.Authorization;
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Services.DtoMapping;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;
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
       
        [HttpPost("modify")] // POST /api/team/modify
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        [ProducesResponseType(typeof(ApiResponse), 501)]

        public async Task<ActionResult<ApiResponse>> SaveSkillsAsync(
           [FromBody] ModifySkillRequest dto,
           CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");
            if (userId == null)
                return Unauthorized();

            if  (dto.SkillType>2 || dto.SkillType<1 || dto.MemberId>8)
                return BadRequest(ApiResponse.Fail(_localizer["Resp.Error.InValidData"].Value));

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);

            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var success = await _teamService.SaveModifiedSkillAsync(playerId.Value, dto, ct);
            
            if (success == null)
            {
                _logger.LogWarning($"Session ID probléma user:{userId} sessionId:", dto.SessionId);
                return StatusCode(501, ApiResponse.Fail(_localizer["Error.Session"].Value));
            }
            else if (success == false)
            {
                _logger.LogWarning($"Skill modosítás sikertelen. userId={userId}", userId);
                return StatusCode(500, ApiResponse.Fail(_localizer["Error.Internal"].Value));
            }
            else
                return Ok(ApiResponse.Ok(_localizer["Resp.SaveOk"].Value));
        }

        [HttpPost("manage")] // POST /api/team/manage
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        [ProducesResponseType(typeof(ApiResponse), 501)]
        public async Task<ActionResult<ApiResponse>> ManageTeamAsync(
           [FromBody] ManageTeamRequest dto,
           CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub")
                    ?? throw new InvalidOperationException("Missing user id");
            if (userId == null)
                return Unauthorized();

            if ((int)dto.ReqType>4 || (int)dto.ReqType<0)
                return BadRequest(ApiResponse.Fail(_localizer["Resp.Error.InvalidRequest"].Value));

            if (dto.MemberNo < 1 || dto.MemberNo > 8) 
                return BadRequest(ApiResponse.Fail(_localizer["Resp.Error.InvalidMember"].Value)); 

            if ((int)dto.ReqType == 0 && (dto.CandidateId < 1 || dto.CandidateId > 8))
                return BadRequest(ApiResponse.Fail(_localizer["Resp.Error.InvalidCandidate"].Value));


            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");


            var success = await _teamService.ManageTeamAsync(playerId.Value, dto, ct);
           
           
            if (success == null)
            {
                _logger.LogWarning($"Session ID probléma user:{userId} sessionId:", dto.SessionId);
                return StatusCode(501, ApiResponse.Fail(_localizer["Error.Session"].Value));
            }
            else if (success == false)
            {
                _logger.LogWarning($"Csapatmodositás sikertelen ({dto.ReqType.ToString()}) sikertelen. userId={userId}", userId);
                return StatusCode(500, ApiResponse.Fail(_localizer["Error.Internal"].Value));
            }
            else
                return Ok(ApiResponse.Ok(_localizer["Resp.SaveOk"].Value));

        }



        public sealed record ApiResponse(bool Success, string Message, string? ServerVersion = null)
        {
            public static ApiResponse Ok(string msg) => new(true, msg);
            public static ApiResponse Fail(string msg) => new(false, msg);
        }
    }

}
