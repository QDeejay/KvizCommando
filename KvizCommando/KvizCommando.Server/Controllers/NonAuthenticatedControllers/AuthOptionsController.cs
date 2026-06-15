using KvizCommando.Server.Identity;
using KvizCommando.Shared.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KvizCommando.Server.Controllers.NonAuthenticatedControllers;

[ApiController]
[Route("api/auth/options")]
[AllowAnonymous]
public class AuthOptionsController : ControllerBase
{
    private readonly IdentityOptions _options;

    public AuthOptionsController(IOptions<IdentityOptions> options)
    {
        _options = options.Value;
    }

    [HttpGet]
    public ActionResult<RegisterOptionsResponse> GetOptions()
    {
        var dto = new RegisterOptionsResponse
        {
            // Password policy
            RequiredLength = _options.Password.RequiredLength,
            RequireNonAlphanumeric = _options.Password.RequireNonAlphanumeric,
            RequireDigit = _options.Password.RequireDigit,
            RequireLowercase = _options.Password.RequireLowercase,
            RequireUppercase = _options.Password.RequireUppercase,
            RequiredUniqueChars = _options.Password.RequiredUniqueChars,

            // User policy
            RequireUniqueEmail = _options.User.RequireUniqueEmail,

            // SignIn policy
            RequireConfirmedEmail = _options.SignIn.RequireConfirmedEmail,
            RequireConfirmedAccount = _options.SignIn.RequireConfirmedAccount,

            // Custom rules
            DisplayNameMaxLength = CheckInValidationOptions.DisplayNameMaxLength,
            DisplayNameMinLength = CheckInValidationOptions.DisplayNameMinLength,
            //AcceptTermsRequired = RegistrationValidationOptions.AcceptTermsRequired,
            //PreferredLocaleRegex = RegistrationValidationOptions.PreferredLocaleRegex
        };

        return Ok(dto);
    }
}
