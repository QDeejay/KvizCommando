using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace KvizCommando.Server.Infrastructure.Logging;

/// <summary>
/// Napi JSONL-fájlokba írja a biztonsági auditbejegyzéseket.
/// A helyi fájl nem helyettesít változtathatatlan, központi production auditot.
/// </summary>
public sealed class FileAuditLogger : IAuditLogger, IDisposable
{
    private readonly AuditOptions _options;
    private readonly ILogger<FileAuditLogger> _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly byte[]? _ipHashKey;
    private DateOnly? _lastCleanupDate;

    public FileAuditLogger(
        IOptions<AuditOptions> options,
        IConfiguration configuration,
        ILogger<FileAuditLogger> logger)
    {
        _options = options.Value;
        _logger = logger;
        _ipHashKey = ReadHashKey(configuration["AuditHash:Secret"]);
    }

    /// <inheritdoc />
    public async Task LogAsync(
        AuditEntry entry,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var serializedEntry = JsonSerializer.Serialize(new
        {
            utcTime = now,
            eventName = entry.EventName,
            outcome = entry.Outcome.ToString(),
            subjectId = entry.SubjectId,
            ipHash = GetIpHash(entry.IpAddress),
            requestId = entry.RequestId
        });

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(_options.OutputRoot);
            CleanupExpiredFiles(now);

            var filePath = Path.Combine(
                _options.OutputRoot,
                $"audit-{now:yyyy-MM-dd}.jsonl");
            await File.AppendAllTextAsync(
                filePath,
                serializedEntry + Environment.NewLine,
                Encoding.UTF8,
                cancellationToken);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            // Az auditfájl hibája látható marad az alkalmazásnaplóban, de nem töri meg az Identity-műveletet.
            _logger.LogError(
                exception,
                "Nem sikerült kiírni a(z) {AuditEvent} auditbejegyzést.",
                entry.EventName);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public void Dispose()
    {
        _writeLock.Dispose();
    }

    private void CleanupExpiredFiles(DateTimeOffset now)
    {
        var currentDate = DateOnly.FromDateTime(now.UtcDateTime);
        if (_lastCleanupDate == currentDate)
        {
            return;
        }

        _lastCleanupDate = currentDate;
        var oldestAllowedWriteTime = now.UtcDateTime.AddDays(-_options.RetentionDays);

        foreach (var filePath in Directory.EnumerateFiles(
                     _options.OutputRoot,
                     "audit-*.jsonl",
                     SearchOption.TopDirectoryOnly))
        {
            try
            {
                if (File.GetLastWriteTimeUtc(filePath) < oldestAllowedWriteTime)
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(
                    exception,
                    "Nem sikerült eltávolítani a lejárt auditfájlt: {AuditFile}",
                    filePath);
            }
        }
    }

    private string? GetIpHash(string? ipAddress)
    {
        if (!_options.IncludeIpHash ||
            _ipHashKey is null ||
            string.IsNullOrWhiteSpace(ipAddress))
        {
            return null;
        }

        var hash = HMACSHA256.HashData(
            _ipHashKey,
            Encoding.UTF8.GetBytes(ipAddress));
        return Convert.ToHexString(hash);
    }

    private byte[]? ReadHashKey(string? configuredValue)
    {
        if (string.IsNullOrWhiteSpace(configuredValue))
        {
            if (_options.IncludeIpHash)
            {
                _logger.LogWarning(
                    "Az Audit:IncludeIpHash aktív, de az AuditHash:Secret nincs beállítva; az IP-hash kimarad.");
            }

            return null;
        }

        try
        {
            return Convert.FromBase64String(configuredValue);
        }
        catch (FormatException)
        {
            _logger.LogWarning(
                "Az AuditHash:Secret nem érvényes Base64-érték; az IP-hash kimarad.");
            return null;
        }
    }
}
