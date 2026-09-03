using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace KvizCommando.Admin;

internal sealed class DeploymentOperations
{
    private const string RELEASES_ROOT = "/var/www/kvizcommando-releases";
    private const string CURRENT_LINK = "/var/www/kvizcommando";
    private const string DEPLOY_LOG_PATH = "/var/log/kvizcommando/deploy.log";
    private const string MIGRATION_MANIFEST_PATH = "/var/www/kvizcommando-migrations/last-upload.json";

    private static readonly JsonSerializerOptions JSON_OPTIONS = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AdminDatabase _database;

    public DeploymentOperations(AdminDatabase database)
    {
        _database = database;
    }

    public DeploymentSnapshot GetSnapshot()
    {
        EnsureProduction();
        var migration = GetMigrationSnapshot();
        var activePath = GetActiveReleasePath();
        var deployTimes = ReadDeployTimes();
        var releases = Directory.Exists(RELEASES_ROOT)
            ? Directory
                .EnumerateDirectories(RELEASES_ROOT, "*", SearchOption.TopDirectoryOnly)
                .Select(path => CreateRelease(path, activePath, deployTimes, migration))
                .OrderByDescending(release => release.DeployedAtUtc)
                .ToArray()
            : Array.Empty<ReleaseRow>();

        return new DeploymentSnapshot(SystemOperations.GetServerState(), migration, releases);
    }

    public void ActivateRelease(ReleaseRow release)
    {
        EnsureProduction();
        if (!SystemOperations.IsServerStopped())
            throw new InvalidOperationException("Release csak leállított szervernél aktiválható.");
        if (release.IsActive)
            throw new InvalidOperationException("Ez a release már aktív.");

        var path = ValidateReleasePath(release.Id);
        RunRequired("sudo", "ln", "-sfn", "--", path, CURRENT_LINK);
    }

    public void DeleteRelease(ReleaseRow release)
    {
        EnsureProduction();
        if (release.IsActive || PathsEqual(release.DirectoryPath, GetActiveReleasePath()))
            throw new InvalidOperationException("Az aktív release nem törölhető.");

        var path = ValidateReleasePath(release.Id);
        RunRequired("sudo", "rm", "-rf", "--", path);
    }

    private MigrationSnapshot? GetMigrationSnapshot()
    {
        if (!File.Exists(MIGRATION_MANIFEST_PATH))
            return null;

        var manifest = JsonSerializer.Deserialize<MigrationUploadManifest>(
            File.ReadAllText(MIGRATION_MANIFEST_PATH),
            JSON_OPTIONS)
            ?? throw new InvalidDataException("A last-upload.json tartalma üres.");

        var application = GetTargetStatus(
            manifest.Application,
            _database.IsApplicationMigrationApplied);
        var game = GetTargetStatus(
            manifest.Game,
            _database.IsGameMigrationApplied);

        return new MigrationSnapshot(manifest.UploadedAtUtc, application, game);
    }

    private static MigrationTargetState GetTargetStatus(
        MigrationUploadTarget target,
        Func<string, bool> isApplied)
    {
        if (!target.Required)
            return new MigrationTargetState(false, null, null, MigrationExecutionState.NotRequired);
        if (string.IsNullOrWhiteSpace(target.Migration))
            throw new InvalidDataException("A kötelező migráció azonosítója hiányzik a last-upload.json fájlból.");

        return new MigrationTargetState(
            true,
            target.Migration,
            target.File,
            isApplied(target.Migration)
                ? MigrationExecutionState.Applied
                : MigrationExecutionState.Pending);
    }

    private static ReleaseRow CreateRelease(
        string path,
        string? activePath,
        IReadOnlyDictionary<string, DateTimeOffset> deployTimes,
        MigrationSnapshot? migration)
    {
        var id = Path.GetFileName(path);
        var directory = new DirectoryInfo(path);
        var deployedAtUtc = deployTimes.TryGetValue(id, out var loggedAt)
            ? loggedAt
            : new DateTimeOffset(directory.LastWriteTimeUtc, TimeSpan.Zero);
        var hasMigrationRisk = migration is not null &&
                               migration.HasAppliedMigration &&
                               deployedAtUtc < migration.UploadedAtUtc;

        return new ReleaseRow(
            id,
            path,
            PathsEqual(path, activePath),
            deployedAtUtc,
            hasMigrationRisk);
    }

    private static string? GetActiveReleasePath()
    {
        var link = new DirectoryInfo(CURRENT_LINK);
        if (link.LinkTarget is null)
            return null;

        return link.ResolveLinkTarget(returnFinalTarget: true)?.FullName;
    }

    private static IReadOnlyDictionary<string, DateTimeOffset> ReadDeployTimes()
    {
        var result = new Dictionary<string, DateTimeOffset>(StringComparer.Ordinal);
        if (!File.Exists(DEPLOY_LOG_PATH))
            return result;

        foreach (var line in File.ReadLines(DEPLOY_LOG_PATH))
        {
            const string marker = " - Release: ";
            var markerIndex = line.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex <= 0)
                continue;

            var timestampText = line[..markerIndex];
            var releaseId = line[(markerIndex + marker.Length)..].Trim();
            if (DateTime.TryParseExact(
                    timestampText,
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal,
                    out var localTime))
            {
                result[releaseId] = new DateTimeOffset(localTime).ToUniversalTime();
            }
        }

        return result;
    }

    private static string ValidateReleasePath(string releaseId)
    {
        if (string.IsNullOrWhiteSpace(releaseId) ||
            releaseId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            releaseId.Contains(Path.DirectorySeparatorChar) ||
            releaseId.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new InvalidOperationException("Érvénytelen release azonosító.");
        }

        var root = Path.GetFullPath(RELEASES_ROOT);
        var path = Path.GetFullPath(Path.Combine(root, releaseId));
        if (!string.Equals(Path.GetDirectoryName(path), root, StringComparison.Ordinal) ||
            !Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Nem található release: {releaseId}");
        }

        return path;
    }

    private static bool PathsEqual(string? left, string? right) =>
        !string.IsNullOrWhiteSpace(left) &&
        !string.IsNullOrWhiteSpace(right) &&
        string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
            StringComparison.Ordinal);

    private static void RunRequired(string fileName, params string[] arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? output : error);
    }

    private static void EnsureProduction()
    {
        if (!OperatingSystem.IsLinux())
            throw new InvalidOperationException("A deploy funkciók csak a production Linux VM-en használhatók.");
    }

    private sealed record MigrationUploadManifest(
        DateTimeOffset UploadedAtUtc,
        MigrationUploadTarget Application,
        MigrationUploadTarget Game);

    private sealed record MigrationUploadTarget(
        bool Required,
        string? Migration,
        string? File);
}
