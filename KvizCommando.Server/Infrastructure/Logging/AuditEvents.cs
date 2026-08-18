namespace KvizCommando.Server.Infrastructure.Logging;

public static class AuditEvents
{
    public const string AccountRegistered = "Account.Registered";
    public const string AccountDeactivated = "Account.Deactivated";
    public const string AccountDeleted = "Account.Deleted";

    public const string LoginSucceeded = "Auth.LoginSucceeded";
    public const string LoginFailed = "Auth.LoginFailed";
    public const string AccountLocked = "Auth.AccountLocked";
    public const string Logout = "Auth.Logout";
    public const string SessionReplaced = "Auth.SessionReplaced";
    public const string SessionRevoked = "Auth.SessionRevoked";

    public const string PasswordChanged = "Identity.PasswordChanged";
    public const string PasswordResetRequested = "Identity.PasswordResetRequested";
    public const string PasswordResetSucceeded = "Identity.PasswordResetSucceeded";
    public const string PasswordResetFailed = "Identity.PasswordResetFailed";
    public const string EmailChanged = "Identity.EmailChanged";
    public const string ExternalLoginLinked = "Identity.ExternalLoginLinked";
    public const string ExternalLoginRemoved = "Identity.ExternalLoginRemoved";

    public const string TermsAccepted = "Privacy.TermsAccepted";
    public const string MarketingConsentGranted = "Privacy.MarketingConsentGranted";
    public const string MarketingConsentWithdrawn = "Privacy.MarketingConsentWithdrawn";

    // A még nem létező GDPR-folyamatok számára fenntartott eseménynevek.
    public const string DataExportRequested = "Privacy.DataExportRequested";
    public const string DataExportCompleted = "Privacy.DataExportCompleted";
    public const string DataExportFailed = "Privacy.DataExportFailed";
    public const string RectificationRequested = "Privacy.RectificationRequested";
    public const string RectificationCompleted = "Privacy.RectificationCompleted";
    public const string RectificationFailed = "Privacy.RectificationFailed";
    public const string ErasureRequested = "Privacy.ErasureRequested";
    public const string ErasureCompleted = "Privacy.ErasureCompleted";
    public const string ErasureFailed = "Privacy.ErasureFailed";
    public const string RestrictionRequested = "Privacy.RestrictionRequested";
    public const string RestrictionCompleted = "Privacy.RestrictionCompleted";
    public const string ObjectionRecorded = "Privacy.ObjectionRecorded";
}
