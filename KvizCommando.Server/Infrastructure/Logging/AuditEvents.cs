namespace KvizCommando.Server.Infrastructure.Logging;

public static class AuditEvents
{
    public const string ACCOUNT_REGISTERED = "Account.Registered";
    public const string ACCOUNT_DEACTIVATED = "Account.Deactivated";
    public const string ACCOUNT_DELETED = "Account.Deleted";

    public const string LOGIN = "Auth.Login";
    public const string ACCOUNT_LOCKED = "Auth.AccountLocked";
    public const string LOGOUT = "Auth.Logout";
    public const string SESSION_REPLACED = "Auth.SessionReplaced";
    public const string SESSION_REVOKED = "Auth.SessionRevoked";

    public const string PASSWORD_CHANGED = "Identity.PasswordChanged";
    public const string PASSWORD_RESET_REQUESTED = "Identity.PasswordResetRequested";
    public const string PASSWORD_RESET = "Identity.PasswordReset";
    public const string EMAIL_CHANGED = "Identity.EmailChanged";
    public const string EXTERNAL_LOGIN_LINKED = "Identity.ExternalLoginLinked";
    public const string EXTERNAL_LOGIN_REMOVED = "Identity.ExternalLoginRemoved";

    public const string TERMS_ACCEPTED = "Privacy.TermsAccepted";
    public const string MARKETING_CONSENT_GRANTED = "Privacy.MarketingConsentGranted";
    public const string MARKETING_CONSENT_WITHDRAWN = "Privacy.MarketingConsentWithdrawn";

    public const string DATA_EXPORT = "Privacy.DataExport";
    public const string ERASURE = "Privacy.Erasure";

    // A még nem létező érintetti folyamatok számára fenntartott eseménynevek.
    public const string RECTIFICATION = "Privacy.Rectification";
    public const string RESTRICTION = "Privacy.Restriction";
    public const string OBJECTION_RECORDED = "Privacy.ObjectionRecorded";
}
