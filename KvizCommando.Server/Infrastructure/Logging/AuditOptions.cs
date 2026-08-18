namespace KvizCommando.Server.Infrastructure.Logging;

public sealed class AuditOptions
{
    public const string SectionName = "Audit";

    public string Provider { get; set; } = "File";
    public string OutputRoot { get; set; } = @"C:\KvizCommando\Audit";
    public int RetentionDays { get; set; } = 30;
    public bool IncludeIpHash { get; set; }
}
