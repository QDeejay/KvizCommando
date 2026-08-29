using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Email;
using KvizCommando.Shared.Contracts.Auth;
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
    private readonly EmailOptions _emailOptions;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public AuthOptionsController(
        IOptions<IdentityOptions> options,
        IOptions<EmailOptions> emailOptions,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _options = options.Value;
        _emailOptions = emailOptions.Value;
        _environment = environment;
        _configuration = configuration;
    }

    /// <summary>
    /// Visszaadja a regisztráció és beléptetés kliensoldali szabályait.
    /// </summary>
    [HttpGet]
    public ActionResult<RegisterOptionsResponse> GetOptions()
    {
        var dto = new RegisterOptionsResponse
        {
            // A kliens ugyanazokat a jelszószabályokat jeleníti meg, amelyeket az Identity érvényesít.
            RequiredLength = _options.Password.RequiredLength,
            RequireNonAlphanumeric = _options.Password.RequireNonAlphanumeric,
            RequireDigit = _options.Password.RequireDigit,
            RequireLowercase = _options.Password.RequireLowercase,
            RequireUppercase = _options.Password.RequireUppercase,
            RequiredUniqueChars = _options.Password.RequiredUniqueChars,

            RequireUniqueEmail = _options.User.RequireUniqueEmail,

            RequireConfirmedEmail = _options.SignIn.RequireConfirmedEmail,
            RequireConfirmedAccount = _options.SignIn.RequireConfirmedAccount,
            RegistrationEnabled = IdentityConfiguration.IsRegistrationEnabled(
                _configuration),
            FacebookLoginEnabled = IdentityConfiguration.IsFacebookLoginEnabled(
                _configuration),

            DisplayNameMaxLength = CheckInValidationOptions.DISPLAY_NAME_MAX_LENGTH,
            DisplayNameMinLength = CheckInValidationOptions.DISPLAY_NAME_MIN_LENGTH,
        };

        if (_environment.IsDevelopment() &&
            string.Equals(
                _emailOptions.Service,
                EmailOptions.FILE_SERVICE,
                StringComparison.OrdinalIgnoreCase))
        {
            var projectDirectory = new DirectoryInfo(
                _environment.ContentRootPath).Name;
            dto.RegistrationEmailOutputPath = Path.Combine(
                projectDirectory,
                _emailOptions.OutputRoot,
                FileEmailDelivery.GetDirectoryName(
                    EmailMessageType.Registration));
            dto.PasswordResetEmailOutputPath = Path.Combine(
                projectDirectory,
                _emailOptions.OutputRoot,
                FileEmailDelivery.GetDirectoryName(
                    EmailMessageType.PasswordReset));
        }

        return Ok(dto);
    }
}
