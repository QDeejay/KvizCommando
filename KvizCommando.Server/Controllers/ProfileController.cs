using KvizCommando.Server.Authorization;
using KvizCommando.Server.Services.Profile;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Shared.Contracts.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KvizCommando.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = TermsAcceptedRequirement.POLICY_NAME)]
public sealed class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IUserPlayerIdCacheService _idCache;

    public ProfileController(
        IProfileService profileService,
        IUserPlayerIdCacheService idCache)
    {
        _profileService = profileService;
        _idCache = idCache;
    }

    [HttpGet]
    public async Task<ActionResult<ProfileLoadResponse>> GetAsync(
        [FromQuery] string sessionId,
        CancellationToken ct)
    {
        var playerId = await GetPlayerIdAsync(ct);
        if (playerId is null)
            return NotFound();

        return Ok(await _profileService.GetAsync(
            playerId.Value,
            sessionId,
            ct));
    }

    [HttpPost("check-teamname")]
    public async Task<ActionResult<CheckTeamNameResponse>> CheckTeamNameAsync(
        [FromBody] CheckTeamNameRequest request,
        CancellationToken ct)
    {
        var playerId = await GetPlayerIdAsync(ct);
        if (playerId is null)
            return NotFound();

        return Ok(await _profileService.CheckTeamNameAsync(
            playerId.Value,
            request,
            ct));
    }

    [HttpPost("teamname")]
    public async Task<ActionResult<SaveProfileResponse>> SaveTeamNameAsync(
        [FromBody] SaveTeamNameRequest request,
        CancellationToken ct)
    {
        var playerId = await GetPlayerIdAsync(ct);
        if (playerId is null)
            return NotFound();

        return Ok(await _profileService.SaveTeamNameAsync(
            playerId.Value,
            request,
            ct));
    }

    [HttpPost("avatar")]
    public async Task<ActionResult<SaveProfileResponse>> SaveAvatarAsync(
        [FromBody] SaveAvatarRequest request,
        CancellationToken ct)
    {
        var playerId = await GetPlayerIdAsync(ct);
        if (playerId is null)
            return NotFound();

        return Ok(await _profileService.SaveAvatarAsync(
            playerId.Value,
            request,
            ct));
    }

    private async Task<int?> GetPlayerIdAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                     User.FindFirstValue("sub");

        return string.IsNullOrWhiteSpace(userId)
            ? null
            : await _idCache.GetPlayerIdAsync(userId, ct);
    }
}
