using KvizCommando.Server.Authorization;
using KvizCommando.Server.Services.DtoMapping;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KvizCommando.Server.Controllers.ScreenControllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = TermsAcceptedRequirement.POLICY_NAME)]
    public class ScreenController : ControllerBase
    {
        private readonly IScreenService _screenService;
        private readonly IUserPlayerIdCacheService _idCache;
        public ScreenController(IScreenService screenService, IUserPlayerIdCacheService userPlayerId)
        {
            _screenService = screenService;
            _idCache = userPlayerId;
        }


        /// <summary>
        /// Lekéri a kezdőképernyő megjelenítéséhez szükséges adatokat.
        /// </summary>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        [HttpGet("home")]
        [ProducesResponseType(typeof(HomeDTOs), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<HomeDTOs>> GetHomeScreenAsync([FromQuery] string sessionId, CancellationToken ct)

        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");
            if (userId == null)
                return Unauthorized();

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var dto = await _screenService.GetHomeScreenAsync(playerId.Value, sessionId, ct);

            if (dto == null)
                return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Lekéri a kérdéskezelő képernyő megjelenítési adatait.
        /// </summary>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        [HttpGet("question")]
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

            var dto = await _screenService.GetQuestionScreenAsync(playerId.Value, sessionId, ct);
            if (dto is null)
                return NotFound();

            return Ok(dto);
        }




        /// <summary>
        /// Lekéri a csapatképernyő megjelenítési adatait.
        /// </summary>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        [HttpGet("team")]
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

            var dto = await _screenService.GetTeamScreenDataAsync(playerId.Value, sessionId, ct);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }



        /// <summary>
        /// Lekéri az egyéni játék választóképernyőjének adatait.
        /// </summary>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        [HttpGet("sologame")]
        [ProducesResponseType(typeof(SoloGameDtos), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SoloGameDtos>> GetSoloScreenAsync([FromQuery] string sessionId, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");
            if (userId == null)
                return Unauthorized();
            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var dto = await _screenService.GetSoloGameScreenAsync(playerId.Value, sessionId, ct);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Lekéri a többjátékos mód választóképernyőjének adatait.
        /// </summary>
        /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        [HttpGet("vsgame")]
        [ProducesResponseType(typeof(VsGameDtos), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VsGameDtos>> GetVsGameScreenAsync([FromQuery] string sessionId, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

            if (userId == null)
                return Unauthorized();

            var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
            if (playerId is null or 0)
                return NotFound("No Player record found for this user.");

            var dto = await _screenService.GetVsGameScreenAsync(
                playerId.Value,
                sessionId,
                ct);

            if (dto == null)
                return NotFound();

            return Ok(dto);
        }


    }
}
