namespace KvizCommando.Server.Infrastructure.Logging;

public enum AuditOutcome
{
    Accepted,
    Succeeded,
    Failed
}

public sealed record AuditEntry(
    string EventName,
    AuditOutcome Outcome,
    string? SubjectId,
    string? IpAddress,
    string? RequestId);
