using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KvizCommando.Admin;

internal sealed record PublicAuthState(bool RegistrationEnabled, bool FacebookLoginEnabled);

internal static class ProductionOperations
{
    private const string OPERATIONS_PATH = "/etc/kvizcommando/operations.json";
    private const string SITE_MODE_COMMAND = "/usr/local/sbin/kvizcommando-site-mode";

    public static PublicAuthState GetPublicAuthState()
    {
        EnsureLinux();
        var root = LoadOperations();
        var publicAuth = root["PublicAuth"] as JsonObject;

        return new PublicAuthState(
            publicAuth?["RegistrationEnabled"]?.GetValue<bool>() ?? false,
            publicAuth?["FacebookLoginEnabled"]?.GetValue<bool>() ?? false);
    }

    public static void SetRegistrationEnabled(bool enabled) =>
        UpdatePublicAuth("RegistrationEnabled", enabled);

    public static void SetFacebookLoginEnabled(bool enabled) =>
        UpdatePublicAuth("FacebookLoginEnabled", enabled);

    public static string GetSiteMode()
    {
        EnsureLinux();
        var result = Run(SITE_MODE_COMMAND, "status");
        if (result.ExitCode != 0)
            return "UNKNOWN";

        return string.IsNullOrWhiteSpace(result.Output)
            ? "UNKNOWN"
            : result.Output.Trim().ToUpperInvariant();
    }

    public static void SetSiteOnline() => RunRequired(SITE_MODE_COMMAND, "online");
    public static void SetSiteMaintenance() => RunRequired(SITE_MODE_COMMAND, "maintenance");

    private static void UpdatePublicAuth(string propertyName, bool enabled)
    {
        EnsureLinux();
        var root = LoadOperations();
        var publicAuth = root["PublicAuth"] as JsonObject;
        if (publicAuth is null)
        {
            publicAuth = new JsonObject();
            root["PublicAuth"] = publicAuth;
        }

        publicAuth[propertyName] = enabled;

        var tempPath = OPERATIONS_PATH + ".tmp";
        File.WriteAllText(tempPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        File.Move(tempPath, OPERATIONS_PATH, overwrite: true);
    }

    private static JsonObject LoadOperations()
    {
        if (!File.Exists(OPERATIONS_PATH))
            throw new FileNotFoundException($"Nem található az operations fájl: {OPERATIONS_PATH}");

        return JsonNode.Parse(File.ReadAllText(OPERATIONS_PATH)) as JsonObject
               ?? throw new InvalidOperationException("Az operations.json gyökere nem JSON objektum.");
    }

    private static void RunRequired(string fileName, string arguments)
    {
        var result = Run(fileName, arguments);
        if (result.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.Error) ? result.Output : result.Error);
    }

    private static ProcessResult Run(string fileName, string arguments)
    {
        if (!File.Exists(fileName))
            throw new FileNotFoundException($"Nem található a site-mode parancs: {fileName}");

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return new ProcessResult(process.ExitCode, output, error);
    }

    private static void EnsureLinux()
    {
        if (!OperatingSystem.IsLinux())
            throw new InvalidOperationException("A production operations funkciók csak a Linux VM-en használhatók.");
    }

    private sealed record ProcessResult(int ExitCode, string Output, string Error);
}
