/*
 
using KvizCommando.Server.Authorization;
using KvizCommando.Server.Services.DtoMapping;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace KvizCommando.Server.Controllers.ScreenControllers
{
    [ApiController]
    [Route("api/team")]
    [Authorize(Policy = TermsAcceptedRequirement.PolicyName)]
    public class TeamScreenController : ControllerBase
    {
        private readonly ITeamService _teamScreenService;
        private readonly IUserPlayerIdCacheService _idCache;

        public TeamScreenController(ITeamService teamScreenService, IUserPlayerIdCacheService userPlayerId)
        {
            _teamScreenService = teamScreenService;
            _idCache = userPlayerId;
        }
        [HttpGet]
        [ProducesResponseType(typeof(TeamDtos), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TeamDtos>> GetTeamScreenAsync(CancellationToken ct)
        {
            var sessionId = "Teszt";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");
            if (userId == null)
                return Unauthorized();
            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var dto = await _teamScreenService.GetTeamScreenDataAsync(playerId.Value, sessionId, ct);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }
        
    }
}

 
 */