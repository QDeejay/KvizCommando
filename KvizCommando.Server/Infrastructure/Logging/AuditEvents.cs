namespace KvizCommando.Server.Infrastructure.Logging;

public static class AuditEvents
{
    public const string AccountRegistered = "Account.Registered";
    public const string AccountDeactivated = "Account.Deactivated";
    public const string AccountDeleted = "Account.Deleted";

    public const string Login = "Auth.Login";
    public const string AccountLocked = "Auth.AccountLocked";
    public const string Logout = "Auth.Logout";
    public const string SessionReplaced = "Auth.SessionReplaced";
    public const string SessionRevoked = "Auth.SessionRevoked";

    public const string PasswordChanged = "Identity.PasswordChanged";
    public const string PasswordResetRequested = "Identity.PasswordResetRequested";
    public const string PasswordReset = "Identity.PasswordReset";
    public const string EmailChanged = "Identity.EmailChanged";
    public const string ExternalLoginLinked = "Identity.ExternalLoginLinked";
    public const string ExternalLoginRemoved = "Identity.ExternalLoginRemoved";

    public const string TermsAccepted = "Privacy.TermsAccepted";
    public const string MarketingConsentGranted = "Privacy.MarketingConsentGranted";
    public const string MarketingConsentWithdrawn = "Privacy.MarketingConsentWithdrawn";

    // A még nem létező GDPR-folyamatok számára fenntartott eseménynevek.
    public const string DataExport = "Privacy.DataExport";
    public const string Rectification = "Privacy.Rectification";
    public const string Erasure = "Privacy.Erasure";
    public const string Restriction = "Privacy.Restriction";
    public const string ObjectionRecorded = "Privacy.ObjectionRecorded";
}
