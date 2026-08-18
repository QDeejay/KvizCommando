using KvizCommando.Server.Authorization;
using KvizCommando.Server.Extensions;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Server.Services.VsGame;
using KvizCommando.Shared.Contracts.VsGame;
using KvizCommando.Shared.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace KvizCommando.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = TermsAcceptedRequirement.PolicyName)]
public sealed class VsGameController : ControllerBase
{
    private readonly ILogger<VsGameController> _logger;
    private readonly IVsGameService _vsGameService;
    private readonly IStringLocalizer<VsGameController> _localizer;
    private readonly IUserPlayerIdCacheService _idCache;

    public VsGameController(
        ILogger<VsGameController> logger,
        IVsGameService vsGameService,
        IStringLocalizer<VsGameController> localizer,
        IUserPlayerIdCacheService idCache)
    {
        _logger = logger;
        _vsGameService = vsGameService;
        _localizer = localizer;
        _idCache = idCache;
    }

    [HttpPost("battle-team")]
    [Consumes("application/json")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> SaveBattleTeamAsync(
        [FromBody] SaveBattleTeamRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("sub")
                 ?? throw new InvalidOperationException("Missing user id");

        if (string.IsNullOrWhiteSpace(request.SessionId) ||
            request.SelectedSlotNumbers is null)
        {
            Response.AddToast(
                _localizer["VsGame.Error.InvalidTeam"].Value,
                ToastType.Error);
            return BadRequest();
        }

        var playerId = await _idCache.GetPlayerIdAsync(userId, ct);
        if (playerId is null or 0)
            return NotFound("No Player record found for this user.");

        var result = await _vsGameService.SaveBattleTeamAsync(
            playerId.Value,
            request,
            ct);

        if (result == CacheUpdateResult.SessionMismatch)
        {
            _logger.LogWarning(
                "Session ID problem. user={UserId}",
                userId);
            Response.AddToast(
                _localizer["VsGame.Error.Session"].Value,
                ToastType.Error);
            return Conflict();
        }

        if (result == CacheUpdateResult.NotFound)
            return NotFound();

        if (result == CacheUpdateResult.Rejected)
        {
            Response.AddToast(
                _localizer["VsGame.Error.InvalidTeam"].Value,
                ToastType.Error);
            return BadRequest();
        }

        Response.AddToast(
            _localizer["VsGame.Response.TeamSaved"].Value,
            ToastType.Success);
        return NoContent();
    }
}
