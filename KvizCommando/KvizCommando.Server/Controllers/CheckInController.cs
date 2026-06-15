#nullable enable
using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Server.Services.CheckIn;
using KvizCommando.Server.Services.PlayerCache;
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
[Authorize(Policy = "Api")] // ugyanaz a flow, mint a többi authorizált controllerednél (NE rakj ide Terms policy-t)
public sealed class CheckInController : ControllerBase
{
    private readonly ICheckInService _service;
  
    private readonly ApplicationDbContext _db;
    private readonly ITermsProvider _termsProvider;

    public CheckInController(
        ICheckInService service,
        ApplicationDbContext db,
        ITermsProvider termsProvider)
    {
        _service = service;
        _db = db;
        _termsProvider = termsProvider;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CheckInGetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

        var dto = await _service.GetStatusAsync(userId, ct);
       
        
        return Ok(dto);
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CheckInPostResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PostAsync([FromBody] CheckInPostRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? throw new InvalidOperationException("Missing user id");

        // EF execution strategy (tranziens hibákra újrapróbál)
        var strategy = _db.Database.CreateExecutionStrategy();
        var SuggestedName = string.Empty;
        IReadOnlyList<string> errorKeys = Array.Empty<string>();
        await strategy.ExecuteAsync(async () =>
        {
            (errorKeys, SuggestedName) = await _service.CompleteAsync(userId, req, ct);
        });

        // WASM/cookie esetén a service RefreshSignInAsync-et hív → nem kell refresh.
        // Opaque bearer esetén jelzünk a kliensnek, hogy /auth/refresh szükséges.
        var isBearer = (await HttpContext.AuthenticateAsync(IdentityConstants.BearerScheme)).Succeeded;

        var response = new CheckInPostResponse
        {
            Success = errorKeys.Count == 0,
            Errors = errorKeys.ToList(),
            CurrentTerms = _termsProvider.GetCurrentTerms(),
            RequiresTokenRefresh = isBearer,
            SuggestedDisplayName = SuggestedName
        };

        // Szerződés szerint mindig 200 OK + body (Success/Errors/RequiresTokenRefresh).
        return Ok(response);
    }
}
