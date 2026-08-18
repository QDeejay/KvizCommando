#nullable enable
using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Logging;
using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Server.Services.CheckIn;
using KvizCommando.Shared.Contracts.CheckIn;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KvizCommando.Server.Controllers;

[ApiController]
[Route("api/checkin")]
[Authorize(Policy = "Api")] 
public sealed class CheckInController : ControllerBase
{
    private readonly ICheckInService _service;

    private readonly ApplicationDbContext _db;
    private readonly ITermsProvider _termsProvider;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAuditLogger _audit;

    public CheckInController(
        ICheckInService service,
        ApplicationDbContext db,
        ITermsProvider termsProvider,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IAuditLogger audit)
    {
        _service = service;
        _db = db;
        _termsProvider = termsProvider;
        _userManager = userManager;
        _signInManager = signInManager;
        _audit = audit;
    }

    /// <summary>
    /// Lekéri az aktuális beléptetési állapotot.
    /// </summary>
    /// <param name="sessionId">A kliens aktuális munkamenet-azonosítója.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(CheckInGetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAsync([FromQuery] string sessionId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

        var dto = await _service.GetStatusAsync(userId, sessionId, ct);
        if (dto.PreviousSessionReplaced)
            await ReplacePreviousLoginAsync(userId);

        return Ok(dto);
    }

    /// <summary>
    /// Feldolgozza és elmenti a beléptetés során megadott adatokat.
    /// </summary>
    /// <param name="req">A feldolgozandó beléptetési kérés.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CheckInPostResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PostAsync([FromBody] CheckInPostRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

        // Az EF végrehajtási stratégiája a teljes tranzakciót ismétli meg átmeneti adatbázishibánál.
        var strategy = _db.Database.CreateExecutionStrategy();
        var SuggestedName = string.Empty;
        var previousSessionReplaced = false;
        IReadOnlyList<string> errorKeys = Array.Empty<string>();
        await strategy.ExecuteAsync(async () =>
        {
            (errorKeys, SuggestedName, previousSessionReplaced) =
                await _service.CompleteAsync(userId, req, ct);
        });

        if (errorKeys.Count == 0 && previousSessionReplaced)
            await ReplacePreviousLoginAsync(userId);

        // Opaque bearer esetén a kliensnek külön tokenfrissítést kell kérnie a claimváltozás átvételéhez.
        var isBearer = (await HttpContext.AuthenticateAsync(IdentityConstants.BearerScheme)).Succeeded;

        var response = new CheckInPostResponse
        {
            Success = errorKeys.Count == 0,
            Errors = errorKeys.ToList(),
            CurrentTerms = _termsProvider.GetCurrentTerms(),
            RequiresTokenRefresh = isBearer,
            SuggestedDisplayName = SuggestedName,
            PreviousSessionReplaced = previousSessionReplaced
        };

        // A kliens a válasz törzséből kezeli az üzleti hibákat és a tokenfrissítés szükségességét.
        return Ok(response);
    }

    private async Task ReplacePreviousLoginAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId)
                       ?? throw new InvalidOperationException("User not found.");

            var hasCurrentCookie =
                (await HttpContext.AuthenticateAsync(
                    IdentityConstants.ApplicationScheme)).Succeeded;

            var result = await _userManager.UpdateSecurityStampAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", result.Errors.Select(x => x.Description)));
            }

            if (hasCurrentCookie)
                await _signInManager.RefreshSignInAsync(user);
        }
        catch
        {
            await _audit.LogAsync(
                new AuditEntry(
                    AuditEvents.SessionReplaced,
                    AuditOutcome.Failed,
                    userId,
                    userId,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    HttpContext.TraceIdentifier));
            throw;
        }

        await _audit.LogAsync(
            new AuditEntry(
                AuditEvents.SessionReplaced,
                AuditOutcome.Succeeded,
                userId,
                userId,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.TraceIdentifier));
    }
}
