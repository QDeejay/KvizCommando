namespace KvizCommando.Server.Infrastructure.Logging;

public static class AuditEvents
{
    // Forgot password flow
    public const string ForgotPasswordRequested = "ForgotPasswordRequested";
    public const string ForgotPasswordEmailSent = "ForgotPasswordEmailSent";
    public const string ForgotPasswordEmailNotFound = "ForgotPasswordEmailNotFound";

    // Reset password
    public const string PasswordResetAttempted = "PasswordResetAttempted";
    public const string PasswordResetSucceeded = "PasswordResetSucceeded";
    public const string PasswordResetFailed = "PasswordResetFailed";

    // Manage/info (authenticated profile update, incl. password change)
    public const string ManageInfoRequested = "ManageInfoRequested";
    public const string ManageInfoPasswordChangeSucceeded = "ManageInfoPasswordChangeSucceeded";
    public const string ManageInfoPasswordChangeFailed = "ManageInfoPasswordChangeFailed";
}
