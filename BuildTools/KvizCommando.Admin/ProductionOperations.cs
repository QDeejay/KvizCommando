using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KvizCommando.Admin;

internal sealed record PublicAuthState(bool RegistrationEnabled, bool FacebookLoginEnabled);

internal static class ProductionOperations
{
    private const string OPERATIONS_PATH = "/etc/kvizcommando/operations.json";
    private const string ADMIN_SECRETS_PATH = "/etc/kvizcommando/admin-secrets.json";
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
        var result = RunSiteMode("status");
        if (result.ExitCode != 0)
            return "UNKNOWN";

        return string.IsNullOrWhiteSpace(result.Output)
            ? "UNKNOWN"
            : result.Output.Trim().ToUpperInvariant();
    }

    public static void SetSiteOnline() => RunSiteModeRequired("online");
    public static void SetSiteMaintenance() => RunSiteModeRequired("maintenance");

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

    private static (string Username, string Password) LoadWebhostingCredentials()
    {
        if (!File.Exists(ADMIN_SECRETS_PATH))
            throw new FileNotFoundException($"Nem található az admin secrets fájl: {ADMIN_SECRETS_PATH}");

        using var document = JsonDocument.Parse(File.ReadAllText(ADMIN_SECRETS_PATH));
        if (!document.RootElement.TryGetProperty("Webhosting", out var section))
            throw new InvalidOperationException("Az admin-secrets.json nem tartalmaz Webhosting szekciót.");

        var username = section.TryGetProperty("Username", out var userValue) ? userValue.GetString() : null;
        var password = section.TryGetProperty("Password", out var passwordValue) ? passwordValue.GetString() : null;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("A Webhosting Username és Password kitöltése kötelező az admin-secrets.json fájlban.");

        return (username, password);
    }

    private static void RunSiteModeRequired(string argument)
    {
        var result = RunSiteMode(argument);
        if (result.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.Error) ? result.Output : result.Error);
    }

    private static ProcessResult RunSiteMode(string argument)
    {
        if (!File.Exists(SITE_MODE_COMMAND))
            throw new FileNotFoundException($"Nem található a site-mode parancs: {SITE_MODE_COMMAND}");

        var credentials = LoadWebhostingCredentials();
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = SITE_MODE_COMMAND,
                Arguments = argument,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.StartInfo.Environment["KVIZ_WEBHOSTING_USERNAME"] = credentials.Username;
        process.StartInfo.Environment["KVIZ_WEBHOSTING_PASSWORD"] = credentials.Password;

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
