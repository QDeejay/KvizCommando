using System.Diagnostics;
using System.Text;

namespace DatabaseMigrationTool;

internal sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError)
{
    public bool Success => ExitCode == 0;
}

internal static class CommandRunner
{
    public static async Task<CommandResult> RunAsync(
        string fileName,
        IEnumerable<string> arguments,
        string workingDirectory)
    {
        var argumentList = arguments.ToArray();
        Console.WriteLine($"> {fileName} {string.Join(' ', argumentList.Select(ForDisplay))}");

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        foreach (var argument in argumentList)
        {
            startInfo.ArgumentList.Add(argument);
        }

        try
        {
            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var standardOutputTask = process.StandardOutput.ReadToEndAsync();
            var standardErrorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var standardOutput = await standardOutputTask;
            var standardError = await standardErrorTask;

            if (!string.IsNullOrWhiteSpace(standardOutput))
            {
                Console.WriteLine(standardOutput.TrimEnd());
            }

            if (!string.IsNullOrWhiteSpace(standardError))
            {
                Console.Error.WriteLine(standardError.TrimEnd());
            }

            return new CommandResult(process.ExitCode, standardOutput, standardError);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return new CommandResult(-1, string.Empty, ex.ToString());
        }
    }

    private static string ForDisplay(string argument)
    {
        if (argument.Length == 0)
        {
            return "\"\"";
        }

        return argument.Any(char.IsWhiteSpace)
            ? $"\"{argument.Replace("\"", "\\\"")}\""
            : argument;
    }
}
