using System.Text.Json;

namespace DatabaseMigrationTool;

internal static class ToolSettings
{
    private const string SETTINGS_FILE_NAME = "DatabaseMigrationTool.settings.json";
    private const string SSH_TARGET_ENVIRONMENT_VARIABLE = "KVIZCOMMANDO_SSH_TARGET";
    private const string SSH_KEY_ENVIRONMENT_VARIABLE = "KVIZCOMMANDO_SSH_KEY";

    public static bool LoadAndValidate()
    {
        var repoRoot = FindRepoRoot();
        var settingsPath = Path.Combine(
            repoRoot,
            "BuildTools",
            "DatabaseMigrationTool",
            SETTINGS_FILE_NAME);

        if (!File.Exists(settingsPath))
        {
            Console.WriteLine($"Hiányzik a feltöltési beállítás: {settingsPath}");
            Console.WriteLine("A BuildTools\\DatabaseMigrationTool mappában nevezd át a DatabaseMigrationTool.settings.default.json fájlt DatabaseMigrationTool.settings.json névre, majd töltsd ki.");
            return false;
        }

        ToolSettingsModel? settings;

        try
        {
            settings = JsonSerializer.Deserialize<ToolSettingsModel>(File.ReadAllText(settingsPath));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"A beállításfájl nem olvasható: {settingsPath}");
            Console.WriteLine(ex.Message);
            return false;
        }

        if (settings is null ||
            string.IsNullOrWhiteSpace(settings.SshTarget) ||
            string.IsNullOrWhiteSpace(settings.PrivateKeyPath))
        {
            Console.WriteLine($"Hiányos feltöltési beállítás: {settingsPath}");
            Console.WriteLine("Az SshTarget és PrivateKeyPath mezőket ki kell tölteni.");
            return false;
        }

        var privateKeyPath = Environment.ExpandEnvironmentVariables(settings.PrivateKeyPath.Trim());
        if (!File.Exists(privateKeyPath))
        {
            Console.WriteLine($"A privát kulcs nem található: {privateKeyPath}");
            return false;
        }

        Environment.SetEnvironmentVariable(SSH_TARGET_ENVIRONMENT_VARIABLE, settings.SshTarget.Trim());
        Environment.SetEnvironmentVariable(SSH_KEY_ENVIRONMENT_VARIABLE, privateKeyPath);
        return true;
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "KvizCommando.Server", "KvizCommando.Server.csproj")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("A KvizCommando repo gyökere nem található.");
    }

    private sealed class ToolSettingsModel
    {
        public string SshTarget { get; set; } = string.Empty;
        public string PrivateKeyPath { get; set; } = string.Empty;
    }
}
