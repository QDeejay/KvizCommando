using KvizCommando.Server.Authorization;
using KvizCommando.Server.Services.CheckIn;
using KvizCommando.Server.Infrastructure.Logging;
using KvizCommando.Server.Services.Profile;
using KvizCommando.Server.Services.UserPlayerIdCache;
using KvizCommando.Shared.Contracts.CheckIn;
using KvizCommando.Shared.Contracts.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Globalization;

namespace KvizCommando.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = TermsAcceptedRequirement.POLICY_NAME)]
public sealed class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IProfileAccountService _accountService;
    private readonly IProfileDataExportService _dataExportService;
    private readonly IProfileAccountDeletionService _accountDeletionService;
    private readonly IUserPlayerIdCacheService _idCache;
    private readonly ITermsProvider _termsProvider;
    private readonly IAuditLogger _audit;

    public ProfileController(
        IProfileService profileService,
        IProfileAccountService accountService,
        IProfileDataExportService dataExportService,
        IProfileAccountDeletionService accountDeletionService,
        IUserPlayerIdCacheService idCache,
        ITermsProvider termsProvider,
        IAuditLogger audit)
    {
        _profileService = profileService;
        _accountService = accountService;
        _dataExportService = dataExportService;
        _accountDeletionService = accountDeletionService;
        _idCache = idCache;
        _termsProvider = termsProvider;
        _audit = audit;
    }

    /// <summary>Visszaadja az aktuális, kultúrafüggő jogi dokumentum metaadatait.</summary>
    [HttpGet("legal")]
    [ProducesResponseType(typeof(TermsMeta), StatusCodes.Status200OK)]
    public ActionResult<TermsMeta> GetLegalMeta() =>
        Ok(_termsProvider.GetCurrentTerms());

    /// <summary>Jelszavas újrahitelesítés után kiadja a felhasználó személyesadat-exportját.</summary>
    [HttpPost("export")]
    [EnableRateLimiting("login-protect")]
    [Produces("application/zip")]
    public async Task<IActionResult> ExportDataAsync(
        [FromBody] ProfileDataExportRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _dataExportService.ExportAsync(
            userId,
            request.CurrentPassword,
            ct);
        var outcome = result.State switch
        {
            ProfileDataExportServiceState.Success => AuditOutcome.Succeeded,
            ProfileDataExportServiceState.InvalidPassword => AuditOutcome.Denied,
            _ => AuditOutcome.Failed
        };
        await _audit.LogAsync(
            new AuditEntry(
                AuditEvents.DATA_EXPORT,
                outcome,
                userId,
                userId,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.TraceIdentifier),
            ct);

        return result.State switch
        {
            ProfileDataExportServiceState.Success => File(
                result.Content,
                "application/zip",
                result.FileName),
            ProfileDataExportServiceState.InvalidPassword => BadRequest(),
            ProfileDataExportServiceState.NotFound => NotFound(),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>Jelszavas újrahitelesítés után véglegesen törli a felhasználói fiókot.</summary>
    [HttpPost("delete")]
    [EnableRateLimiting("login-protect")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAccountAsync(
        [FromBody] ProfileAccountDeletionRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var state = await _accountDeletionService.DeleteAsync(
            userId,
            request.CurrentPassword,
            ct);
        var outcome = state switch
        {
            ProfileAccountDeletionServiceState.Success => AuditOutcome.Succeeded,
            ProfileAccountDeletionServiceState.InvalidPassword => AuditOutcome.Denied,
            _ => AuditOutcome.Failed
        };
        await _audit.LogAsync(
            new AuditEntry(
                AuditEvents.ERASURE,
                outcome,
                userId,
                userId,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.TraceIdentifier),
            ct);

        if (state == ProfileAccountDeletionServiceState.Success)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return NoContent();
        }

        return state switch
        {
            ProfileAccountDeletionServiceState.InvalidPassword => BadRequest(),
            ProfileAccountDeletionServiceState.NotFound => NotFound(),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
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

    /// <summary>Az aktuális kliensnyelvre frissíti a kommunikációs nyelvet.</summary>
    [HttpPut("preferred-locale")]
    public async Task<ActionResult<ProfileAccountResponse>> UpdatePreferredLocaleAsync(
        CancellationToken ct)
    {
        var userId = GetUserId();
        return userId is null
            ? Unauthorized()
            : Ok(await _accountService.UpdatePreferredLocaleAsync(
                userId,
                CultureInfo.CurrentUICulture.Name,
                ct));
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
