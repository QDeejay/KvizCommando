using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace KvizCommando.Server.Infrastructure.Logging;

public class AuditLogger : IAuditLogger
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public AuditLogger(IWebHostEnvironment env)
    {
        // log file helye pl. App_Data/Audit/audit.log
        var logDir = Path.Combine(env.ContentRootPath, "App_Data", "Audit");
        Directory.CreateDirectory(logDir);
        _filePath = Path.Combine(logDir, "audit.log");
    }

    public Task LogAsync(string eventName, string? userId, string? ipAddress)
    {
        var entry = new
        {
            utcTime = DateTimeOffset.UtcNow.ToString("o"), // ISO 8601
            eventName,
            userId,
            ipHash = ipAddress is not null ? HashIp(ipAddress) : null
        };

        var line = JsonSerializer.Serialize(entry);

        lock (_lock)
        {
            File.AppendAllText(_filePath, line + Environment.NewLine);
        }

        return Task.CompletedTask;
    }

    private static string HashIp(string ip)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(ip);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
