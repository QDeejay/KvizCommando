using System.Diagnostics;

namespace KvizCommando.Admin;

internal static class SystemOperations
{
    private const string SERVICE_NAME = "KvizCommando.Server";

    public static string GetServerState()
    {
        if (!OperatingSystem.IsLinux())
            return "Development: systemd nem elérhető";

        var result = Run("systemctl", $"is-active {SERVICE_NAME}");
        return string.IsNullOrWhiteSpace(result.Output) ? result.Error : result.Output.Trim();
    }

    public static bool IsServerStopped() =>
        string.Equals(GetServerState(), "inactive", StringComparison.OrdinalIgnoreCase);

    public static void StartServer() => RunRequired("sudo", $"systemctl start {SERVICE_NAME}");
    public static void StopServer() => RunRequired("sudo", $"systemctl stop {SERVICE_NAME}");
    public static void RestartServer() => RunRequired("sudo", $"systemctl restart {SERVICE_NAME}");

    private static void RunRequired(string fileName, string arguments)
    {
        if (!OperatingSystem.IsLinux())
            throw new InvalidOperationException("A systemd műveletek csak a Linux VM-en használhatók.");

        var result = Run(fileName, arguments);
        if (result.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.Error) ? result.Output : result.Error);
    }

    private static ProcessResult Run(string fileName, string arguments)
    {
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

    private sealed record ProcessResult(int ExitCode, string Output, string Error);
}
