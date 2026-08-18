namespace KvizCommando.Server.Infrastructure.Logging;

public enum AuditOutcome
{
    Accepted,
    Succeeded,
    Failed,
    Denied
}

public sealed record AuditDetails(
    string[]? ChangedFields = null,
    string? DocumentVersion = null);

public sealed record AuditEntry(
    string EventName,
    AuditOutcome Outcome,
    string? ActorId,
    string? SubjectId,
    string? IpAddress,
    string? RequestId,
    AuditDetails? Details = null);
