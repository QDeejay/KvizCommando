using System.Diagnostics;
using System.Text.RegularExpressions;

namespace KvizCommando.Admin;

internal static class LogOperations
{
    private const string SERVICE_NAME = "KvizCommando.Server";
    private const string DEPLOY_LOG_PATH = "/var/log/kvizcommando/deploy.log";
    private static readonly Regex SERVER_PREFIX = new(
        @"^(?<timestamp>[A-Z][a-z]{2}\s+\d+\s+\d{2}:\d{2}:\d{2})\s+\S+\s+KvizCommando\.Server\[(?<pid>\d+)\]:",
        RegexOptions.Multiline | RegexOptions.CultureInvariant);

    public static string GetServerLast200()
    {
        EnsureLinux();
        return FormatServerLog(RunRequired("journalctl", $"-u {SERVICE_NAME} -n 200 --no-pager"));
    }

    public static Process StartServerLive() =>
        StartLiveProcess("journalctl", $"-u {SERVICE_NAME} -f -n 30 --no-pager");

    public static Process StartDeployLive() =>
        StartLiveProcess("tail", $"-n 50 -F {DEPLOY_LOG_PATH}");

    public static string FormatServerLog(string content) =>
        SERVER_PREFIX.Replace(content, "${timestamp} KC Server [${pid}]:");

    public static void ClearDeployLog()
    {
        EnsureLinux();
        var result = Run("sudo", $"truncate -s 0 {DEPLOY_LOG_PATH}");
        if (result.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.Error) ? result.Output : result.Error);
    }

    private static Process StartLiveProcess(string fileName, string arguments)
    {
        EnsureLinux();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };
        process.Start();
        return process;
    }

    private static string RunRequired(string fileName, string arguments)
    {
        var result = Run(fileName, arguments);
        if (result.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.Error) ? result.Output : result.Error);
        return result.Output;
    }

    private static ProcessResult Run(string fileName, string arguments)
    {
        EnsureLinux();
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
            throw new InvalidOperationException("A production log funkciók csak a Linux VM-en használhatók.");
    }

    private sealed record ProcessResult(int ExitCode, string Output, string Error);
}
