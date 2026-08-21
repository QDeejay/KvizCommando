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
    private readonly IProfileAccountService _accountService;
    private readonly IUserPlayerIdCacheService _idCache;

    public ProfileController(
        IProfileService profileService,
        IProfileAccountService accountService,
        IUserPlayerIdCacheService idCache)
    {
        _profileService = profileService;
        _accountService = accountService;
        _idCache = idCache;
    }

    /// <summary>Betölti a hitelesített felhasználó fiókadatait.</summary>
    [HttpGet("account")]
    public async Task<ActionResult<ProfileAccountResponse>> GetAccountAsync(CancellationToken ct)
    {
        var userId = GetUserId();
        return userId is null
            ? Unauthorized()
            : Ok(await _accountService.GetAsync(userId, ct));
    }

    /// <summary>Elmenti a hitelesített felhasználó kapcsolattartási és számlázási adatait.</summary>
    [HttpPut("account")]
    public async Task<ActionResult<ProfileAccountResponse>> SaveAccountAsync(
        [FromBody] SaveProfileAccountRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        return userId is null
            ? Unauthorized()
            : Ok(await _accountService.SaveAsync(userId, request, ct));
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
        var userId = GetUserId();

        return string.IsNullOrWhiteSpace(userId)
            ? null
            : await _idCache.GetPlayerIdAsync(userId, ct);
    }

    private string? GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub");
}
