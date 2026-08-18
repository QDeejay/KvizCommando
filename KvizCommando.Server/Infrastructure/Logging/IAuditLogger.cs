using System.Threading.Tasks;

namespace KvizCommando.Server.Infrastructure.Logging;

public interface IAuditLogger
{
    Task LogAsync(string eventName, string? userId, string? ipAddress);
}
