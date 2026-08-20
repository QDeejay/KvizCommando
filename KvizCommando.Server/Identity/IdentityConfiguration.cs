using KvizCommando.Shared.Models.Rules;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace KvizCommando.Server.Identity;

public static class IdentityConfiguration
{
    /// <summary>
    /// Beállítja az alkalmazás Identity szabályait.
    /// </summary>
    /// <param name="options">A beállítandó Identity-opciók.</param>
    public static void ConfigureIdentityOptions(IdentityOptions options)
    {
        // Jelszóházirend
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 4;

        // Felhasználói azonosítók
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

        // Bejelentkezési követelmények
        options.SignIn.RequireConfirmedEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        // Sikertelen belépések miatti zárolás
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
    

        // Identity claimtípusok
        options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
        options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
        options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
        options.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;
        options.ClaimsIdentity.SecurityStampClaimType = "AspNet.Identity.SecurityStamp";

        // Identity tokenkiadók
        options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
        options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.ChangePhoneNumberTokenProvider = TokenOptions.DefaultPhoneProvider;
    }


}
public static class CheckInValidationOptions
{
    public const int DISPLAY_NAME_MAX_LENGTH = PublicNameRules.NAME_MAX_LENGTH;
    public const int DISPLAY_NAME_MIN_LENGTH = PublicNameRules.NAME_MIN_LENGTH;
    public const string DISPLAY_NAME_ALLOWED_CHARS = PublicNameRules.NAME_ALLOWED_CHARACTERS;
}
