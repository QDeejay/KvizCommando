namespace KvizCommando.Server.Infrastructure.Email;

public enum EmailMessageType
{
    Registration,
    PasswordReset,
    EmailChange,
    PasswordChanged,
    AccountDeleted
}

public sealed record EmailMessage(
    EmailMessageType Type,
    string To,
    string From,
    string Subject,
    string TextBody,
    string HtmlBody);
