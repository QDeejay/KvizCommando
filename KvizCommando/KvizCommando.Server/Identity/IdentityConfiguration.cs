// src/Server/Identity/IdentityConfiguration.cs
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace KvizCommando.Server.Identity;

public static class IdentityConfiguration
{
    public static void ConfigureIdentityOptions(IdentityOptions options)
    {
        // ===== Password policy =====
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 4;

        // ===== User policy =====
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

        // ===== Sign-in policy =====
        options.SignIn.RequireConfirmedEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        // ===== Lockout policy =====
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
    

        // ===== Claims identity mapping =====
        options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
        options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
        options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
        options.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;
        options.ClaimsIdentity.SecurityStampClaimType = "AspNet.Identity.SecurityStamp";

        // ===== Token providers =====
        options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
        options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.ChangePhoneNumberTokenProvider = TokenOptions.DefaultPhoneProvider;
    }


}
public static class CheckInValidationOptions
{
    public const int DisplayNameMaxLength = 20;
    public const int DisplayNameMinLength = 3;
    public const string DisplayNameAllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
    //public const bool AcceptTermsRequired = true;
    //public const string PreferredLocaleRegex = "^[a-z]{2}(-[A-Z]{2})?$";
}

