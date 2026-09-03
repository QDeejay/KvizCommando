using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DatabaseMigrationTool;

internal sealed class MigrationWorkflow
{
    private const string SQLITE_PROVIDER = "Sqlite";
    private const string SQL_SERVER_PROVIDER = "SqlServer";
    private const string PRODUCTION_UPLOAD_DIRECTORY = "/var/www/kvizcommando-migrations/";
    private const string SSH_TARGET_ENVIRONMENT_VARIABLE = "KVIZCOMMANDO_SSH_TARGET";
    private const string SSH_KEY_ENVIRONMENT_VARIABLE = "KVIZCOMMANDO_SSH_KEY";

    private static readonly Regex MigrationNamePattern =
        new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    private readonly string _repoRoot;
    private readonly string _serverProject;

    private MigrationWorkflow(string repoRoot)
    {
        _repoRoot = repoRoot;
        _serverProject = Path.Combine(repoRoot, "KvizCommando.Server", "KvizCommando.Server.csproj");
    }

    public static MigrationWorkflow Create()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var serverProject = Path.Combine(
                directory.FullName,
                "KvizCommando.Server",
                "KvizCommando.Server.csproj");

            if (File.Exists(serverProject))
            {
                return new MigrationWorkflow(directory.FullName);
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "A KvizCommando repo gyökere nem található. A programot a KvizCommando repón belül futtasd.");
    }

    public async Task RunDevelopmentMigrationAsync()
    {
        Console.Write("Application migration name: ");
        var applicationMigrationName = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Game migration name:        ");
        var gameMigrationName = Console.ReadLine()?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(applicationMigrationName) &&
            string.IsNullOrWhiteSpace(gameMigrationName))
        {
            Console.WriteLine("Nincs megadott migráció. Nincs teendő.");
            return;
        }

        if (!ValidateMigrationName(applicationMigrationName) ||
            !ValidateMigrationName(gameMigrationName))
        {
            return;
        }

        if (!await CheckEfPrerequisitesAsync())
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(applicationMigrationName))
        {
            var applicationSucceeded = await RunDatabasePairAsync(
                applicationMigrationName,
                new MigrationTarget(
                    "SQLite Application",
                    SQLITE_PROVIDER,
                    "SqliteApplicationDbContext",
                    "Data/Migrations/Sqlite/Application"),
                new MigrationTarget(
                    "SQL Server Application",
                    SQL_SERVER_PROVIDER,
                    "SqlServerApplicationDbContext",
                    "Data/Migrations/SqlServer/Application"));

            if (!applicationSucceeded)
            {
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(gameMigrationName))
        {
            var gameSucceeded = await RunDatabasePairAsync(
                gameMigrationName,
                new MigrationTarget(
                    "SQLite Game",
                    SQLITE_PROVIDER,
                    "SqliteGameDbContext",
                    "Data/Migrations/Sqlite/Game"),
                new MigrationTarget(
                    "SQL Server Game",
                    SQL_SERVER_PROVIDER,
                    "SqlServerGameDbContext",
                    "Data/Migrations/SqlServer/Game"));

            if (!gameSucceeded)
            {
                return;
            }
        }

        Console.WriteLine();
        Console.WriteLine("DEVELOPMENT MIGRATION COMPLETED SUCCESSFULLY");
    }

    public async Task GenerateProductionScriptsAsync(bool upload)
    {
        var selection = ReadProductionSelection();
        if (selection is null)
        {
            Console.WriteLine("Production script generálás megszakítva.");
            return;
        }

        if (!await CheckEfPrerequisitesAsync())
        {
            return;
        }

        var generatedScripts = await GenerateProductionScriptsInternalAsync(selection);
        if (generatedScripts is null)
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Production SQL scripts generated:");
        foreach (var script in generatedScripts.Scripts)
            Console.WriteLine($"  {Path.GetRelativePath(_repoRoot, script.FilePath)}");

        if (!upload)
        {
            Console.WriteLine();
            Console.WriteLine("NO PRODUCTION DATABASE CHANGES WERE EXECUTED.");
            return;
        }

        await UploadProductionScriptsAsync(generatedScripts);
    }

    private async Task<bool> RunDatabasePairAsync(
        string migrationName,
        MigrationTarget sqliteTarget,
        MigrationTarget sqlServerTarget)
    {
        if (!await RunMigrationTargetAsync(migrationName, sqliteTarget))
        {
            return false;
        }

        return await RunMigrationTargetAsync(migrationName, sqlServerTarget);
    }

    private async Task<bool> RunMigrationTargetAsync(
        string migrationName,
        MigrationTarget target)
    {
        Console.WriteLine();
        Console.WriteLine($"[{target.DisplayName}]");

        var existingMigration = FindExistingMigration(target, migrationName);
        var createdInThisRun = existingMigration is null;

        if (createdInThisRun)
        {
            Console.WriteLine($"Migration létrehozása: {migrationName}");
            var addResult = await RunEfAsync(
                "migrations", "add", migrationName,
                "--context", target.Context,
                "--project", _serverProject,
                "--startup-project", _serverProject,
                "--output-dir", target.MigrationDirectory,
                "--",
                $"--Database:Provider={target.Provider}");

            if (!addResult.Success)
            {
                Console.WriteLine();
                Console.WriteLine("ADD-MIGRATION FAILED. A folyamat leállt.");
                return false;
            }
        }
        else
        {
            Console.WriteLine(
                $"A migráció már létezik ({Path.GetFileName(existingMigration)}), ezért nem készül újra.");
        }

        Console.WriteLine("Helyi adatbázis frissítése...");
        var updateResult = await RunEfAsync(
            "database", "update",
            "--context", target.Context,
            "--project", _serverProject,
            "--startup-project", _serverProject,
            "--",
            $"--Database:Provider={target.Provider}");

        if (updateResult.Success)
        {
            Console.WriteLine($"{target.DisplayName}: OK");
            return true;
        }

        Console.WriteLine();
        Console.WriteLine($"{target.DisplayName} DATABASE UPDATE FAILED.");

        if (!createdInThisRun)
        {
            Console.WriteLine(
                "A migráció nem ebben a futásban készült, ezért a tool biztonsági okból nem távolítja el automatikusan.");
            Console.WriteLine("A folyamat leállt.");
            return false;
        }

        Console.WriteLine("A most létrehozott migráció visszavonása...");
        var removeResult = await RunEfAsync(
            "migrations", "remove",
            "--context", target.Context,
            "--project", _serverProject,
            "--startup-project", _serverProject,
            "--",
            $"--Database:Provider={target.Provider}");

        if (removeResult.Success)
        {
            Console.WriteLine("Migration cleanup: OK");
            Console.WriteLine("A folyamat leállt.");
            return false;
        }

        Console.WriteLine();
        Console.WriteLine("ERROR: DATABASE UPDATE FAILED");
        Console.WriteLine("ERROR: AUTOMATIC MIGRATION CLEANUP ALSO FAILED");
        Console.WriteLine("Do not create another migration before checking the current migration state.");
        return false;
    }

    private async Task<GeneratedScripts?> GenerateProductionScriptsInternalAsync(
        ProductionMigrationSelection selection)
    {
        var generatedAt = DateTime.Now;
        var timestamp = generatedAt.ToString("yyyyMMdd_HHmmss");
        var outputDirectory = Path.Combine(_repoRoot, "publish-linux", "migration");
        Directory.CreateDirectory(outputDirectory);

        var targets = new List<ProductionScriptTarget>();
        if (selection.ApplicationRequired)
        {
            targets.Add(new ProductionScriptTarget(
                "application",
                "SqlServerApplicationDbContext",
                "Data/Migrations/SqlServer/Application"));
        }

        if (selection.GameRequired)
        {
            targets.Add(new ProductionScriptTarget(
                "game",
                "SqlServerGameDbContext",
                "Data/Migrations/SqlServer/Game"));
        }

        var scripts = new List<GeneratedScript>();
        foreach (var target in targets)
        {
            var finalPath = Path.Combine(outputDirectory, $"{timestamp}_{target.Key}.sql");
            if (File.Exists(finalPath))
            {
                Console.WriteLine("Az adott másodperchez tartozó production SQL fájl már létezik. Futtasd újra a generálást.");
                return null;
            }

            var tempPath = Path.Combine(outputDirectory, $".{timestamp}_{target.Key}.tmp.sql");
            try
            {
                Console.WriteLine($"SQL Server {target.Key} production script generálása...");
                var result = await GenerateScriptAsync(target.Context, tempPath);

                if (!result.Success)
                {
                    Console.WriteLine($"{target.Key.ToUpperInvariant()} SCRIPT GENERATION FAILED. A folyamat leállt.");
                    return null;
                }

                await WriteFinalScriptAsync(tempPath, finalPath, generatedAt, target.Context);
                scripts.Add(new GeneratedScript(
                    target.Key,
                    finalPath,
                    GetLatestMigrationId(target.MigrationDirectory)));
            }
            finally
            {
                DeleteIfExists(tempPath);
            }
        }

        return new GeneratedScripts(scripts, selection);
    }

    private Task<CommandResult> GenerateScriptAsync(string context, string outputPath) =>
        RunEfAsync(
            "migrations", "script",
            "--idempotent",
            "--configuration", "Release",
            "--context", context,
            "--project", _serverProject,
            "--startup-project", _serverProject,
            "--output", outputPath,
            "--",
            $"--Database:Provider={SQL_SERVER_PROVIDER}");

    private async Task UploadProductionScriptsAsync(GeneratedScripts scripts)
    {
        var sshTarget = Environment.GetEnvironmentVariable(SSH_TARGET_ENVIRONMENT_VARIABLE);

        if (string.IsNullOrWhiteSpace(sshTarget))
        {
            Console.Write("SSH target (user@host vagy SSH config alias): ");
            sshTarget = Console.ReadLine()?.Trim();
        }

        if (string.IsNullOrWhiteSpace(sshTarget))
        {
            Console.WriteLine("Nincs SSH target megadva. Feltöltés kihagyva.");
            return;
        }

        var sshKey = Environment.GetEnvironmentVariable(SSH_KEY_ENVIRONMENT_VARIABLE);
        if (string.IsNullOrWhiteSpace(sshKey))
        {
            Console.Write("Private key path (Enter = SSH config/default): ");
            sshKey = Console.ReadLine()?.Trim();
        }

        var arguments = new List<string>();
        if (!string.IsNullOrWhiteSpace(sshKey))
        {
            arguments.Add("-i");
            arguments.Add(sshKey);
        }

        arguments.AddRange(scripts.Scripts.Select(script => script.FilePath));
        arguments.Add($"{sshTarget}:{PRODUCTION_UPLOAD_DIRECTORY}");

        Console.WriteLine();
        Console.WriteLine("Production SQL fájlok feltöltése...");
        var uploadResult = await CommandRunner.RunAsync("scp", arguments, _repoRoot);

        if (!uploadResult.Success)
        {
            Console.WriteLine("SCP UPLOAD FAILED. A production adatbázison semmilyen művelet nem történt.");
            return;
        }

        var manifestPath = await WriteUploadManifestAsync(scripts);
        var manifestArguments = new List<string>();
        if (!string.IsNullOrWhiteSpace(sshKey))
        {
            manifestArguments.Add("-i");
            manifestArguments.Add(sshKey);
        }
        manifestArguments.Add(manifestPath);
        manifestArguments.Add($"{sshTarget}:{PRODUCTION_UPLOAD_DIRECTORY}last-upload.json");

        Console.WriteLine("Migration upload manifest feltöltése...");
        var manifestUploadResult = await CommandRunner.RunAsync("scp", manifestArguments, _repoRoot);
        if (!manifestUploadResult.Success)
        {
            Console.WriteLine("SQL UPLOAD OK, MANIFEST UPLOAD FAILED.");
            Console.WriteLine("Az SQL fájlok a szerveren vannak, de az admin nem tudja ezt a feltöltést követni.");
            return;
        }

        Console.WriteLine($"Upload OK: {PRODUCTION_UPLOAD_DIRECTORY}");
        Console.WriteLine("NO PRODUCTION DATABASE CHANGES WERE EXECUTED.");
    }

    private ProductionMigrationSelection? ReadProductionSelection()
    {
        Console.WriteLine("Melyik production adatbázist érinti a migráció?");
        Console.WriteLine("[1] Application");
        Console.WriteLine("[2] Game");
        Console.WriteLine("[3] Application + Game");
        Console.WriteLine("[0] Cancel");
        Console.Write("Selection: ");

        return Console.ReadLine()?.Trim() switch
        {
            "1" => new ProductionMigrationSelection(true, false),
            "2" => new ProductionMigrationSelection(false, true),
            "3" => new ProductionMigrationSelection(true, true),
            _ => null
        };
    }

    private string GetLatestMigrationId(string relativeDirectory)
    {
        var directory = Path.Combine(
            _repoRoot,
            "KvizCommando.Server",
            relativeDirectory.Replace('/', Path.DirectorySeparatorChar));
        var migration = Directory
            .EnumerateFiles(directory, "*.cs", SearchOption.TopDirectoryOnly)
            .Where(path => !path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.EndsWith("ModelSnapshot.cs", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal)
            .LastOrDefault();

        return migration is null
            ? throw new InvalidOperationException($"Nem található SQL Server migráció: {directory}")
            : Path.GetFileNameWithoutExtension(migration);
    }

    private async Task<string> WriteUploadManifestAsync(GeneratedScripts scripts)
    {
        var application = scripts.Scripts.SingleOrDefault(script => script.Key == "application");
        var game = scripts.Scripts.SingleOrDefault(script => script.Key == "game");
        var manifest = new MigrationUploadManifest(
            DateTimeOffset.UtcNow,
            CreateManifestTarget(scripts.Selection.ApplicationRequired, application),
            CreateManifestTarget(scripts.Selection.GameRequired, game));
        var path = Path.Combine(_repoRoot, "publish-linux", "migration", "last-upload.json");
        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        await File.WriteAllTextAsync(
            path,
            json + Environment.NewLine,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return path;
    }

    private static MigrationUploadTarget CreateManifestTarget(
        bool required,
        GeneratedScript? script) =>
        required
            ? new MigrationUploadTarget(true, script!.MigrationId, Path.GetFileName(script.FilePath))
            : new MigrationUploadTarget(false, null, null);

    private async Task<bool> CheckEfPrerequisitesAsync()
    {
        Console.WriteLine("Környezet ellenőrzése...");

        var dotnetResult = await CommandRunner.RunAsync("dotnet", new[] { "--version" }, _repoRoot);
        if (!dotnetResult.Success)
        {
            Console.WriteLine("A .NET SDK nem érhető el.");
            return false;
        }

        var efResult = await CommandRunner.RunAsync("dotnet", new[] { "ef", "--version" }, _repoRoot);
        if (!efResult.Success)
        {
            Console.WriteLine("A dotnet-ef tool nem érhető el. Telepítsd a projekthez használt EF Core 8 verziót.");
            return false;
        }

        if (!File.Exists(_serverProject))
        {
            Console.WriteLine($"Server projekt nem található: {_serverProject}");
            return false;
        }

        return true;
    }

    private Task<CommandResult> RunEfAsync(params string[] arguments)
    {
        var allArguments = new List<string> { "ef" };
        allArguments.AddRange(arguments);
        return CommandRunner.RunAsync("dotnet", allArguments, _repoRoot);
    }

    private string? FindExistingMigration(MigrationTarget target, string migrationName)
    {
        var migrationDirectory = Path.Combine(
            _repoRoot,
            "KvizCommando.Server",
            target.MigrationDirectory.Replace('/', Path.DirectorySeparatorChar));

        if (!Directory.Exists(migrationDirectory))
        {
            return null;
        }

        var matches = Directory
            .EnumerateFiles(migrationDirectory, $"*_{migrationName}.cs", SearchOption.TopDirectoryOnly)
            .Where(path => !path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return matches.Length switch
        {
            0 => null,
            1 => matches[0],
            _ => throw new InvalidOperationException(
                $"Több azonos nevű migráció található itt: {migrationDirectory}")
        };
    }

    private static bool ValidateMigrationName(string migrationName)
    {
        if (string.IsNullOrWhiteSpace(migrationName))
        {
            return true;
        }

        if (MigrationNamePattern.IsMatch(migrationName))
        {
            return true;
        }

        Console.WriteLine(
            $"Érvénytelen migrációnév: '{migrationName}'. Csak betű, szám és aláhúzás használható, és a név nem kezdődhet számmal.");
        return false;
    }

    private static async Task WriteFinalScriptAsync(
        string sourcePath,
        string destinationPath,
        DateTime generatedAt,
        string context)
    {
        var sql = await File.ReadAllTextAsync(sourcePath);
        var header =
            $"-- KvizCommando production database migration{Environment.NewLine}" +
            $"-- Generated: {generatedAt:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}" +
            $"-- Context: {context}{Environment.NewLine}{Environment.NewLine}";

        await File.WriteAllTextAsync(
            destinationPath,
            header + sql,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private sealed record MigrationTarget(
        string DisplayName,
        string Provider,
        string Context,
        string MigrationDirectory);

    private sealed record ProductionMigrationSelection(
        bool ApplicationRequired,
        bool GameRequired);

    private sealed record ProductionScriptTarget(
        string Key,
        string Context,
        string MigrationDirectory);

    private sealed record GeneratedScript(
        string Key,
        string FilePath,
        string MigrationId);

    private sealed record GeneratedScripts(
        IReadOnlyList<GeneratedScript> Scripts,
        ProductionMigrationSelection Selection);

    private sealed record MigrationUploadManifest(
        DateTimeOffset UploadedAtUtc,
        MigrationUploadTarget Application,
        MigrationUploadTarget Game);

    private sealed record MigrationUploadTarget(
        bool Required,
        string? Migration,
        string? File);
}
