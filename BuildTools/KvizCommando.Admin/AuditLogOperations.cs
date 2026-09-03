using System.Text.Json;

namespace KvizCommando.Admin;

internal sealed class AuditLogOperations
{
    private static readonly JsonSerializerOptions JSON_OPTIONS = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _outputRoot;

    public AuditLogOperations(string outputRoot)
    {
        _outputRoot = outputRoot;
    }

    public IReadOnlyList<AuditFileRow> GetFiles()
    {
        if (!Directory.Exists(_outputRoot))
            return Array.Empty<AuditFileRow>();

        return Directory
            .EnumerateFiles(_outputRoot, "audit-*.jsonl", SearchOption.TopDirectoryOnly)
            .OrderByDescending(Path.GetFileName)
            .Select(path => new AuditFileRow(path, Path.GetFileName(path)))
            .ToArray();
    }

    public IReadOnlyList<AuditEntryRow> GetEntries(
        AuditFileRow file,
        string? userId = null)
    {
        var entries = new List<AuditEntryRow>();
        var lineNumber = 0;

        foreach (var line in File.ReadLines(file.FilePath))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
                continue;

            AuditEntryRow? entry;
            try
            {
                entry = JsonSerializer.Deserialize<AuditEntryRow>(line, JSON_OPTIONS);
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException(
                    $"Hibás audit JSON: {file.FileName}, {lineNumber}. sor.",
                    exception);
            }

            if (entry is null)
                continue;

            if (!string.IsNullOrWhiteSpace(userId) &&
                !string.Equals(entry.ActorId, userId, StringComparison.Ordinal) &&
                !string.Equals(entry.SubjectId, userId, StringComparison.Ordinal))
            {
                continue;
            }

            entries.Add(entry);
        }

        return entries
            .OrderByDescending(entry => entry.UtcTime)
            .ToArray();
    }
}
