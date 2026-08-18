using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace tosExporter;

internal static class Program
{
    // A segédprogram jelenleg helyi fejlesztői könyvtárakkal működik.
    private const string InputRoot = @"C:\Exports\TermsExporter\Input";
    private const string OutputRoot = @"C:\Exports\TermsExporter\Outputs";

    private static readonly string[] Cultures = { "hu-HU", "en-US" };

    private const string OutputBaseName = "terms";

    // A fájlnév verzióját a minták az itt megadott sorrendben keresik.
    private static readonly Regex RxV123 = new(@"[vV](?<ver>\d+(?:\.\d+){0,2})\b", RegexOptions.CultureInvariant);
    private static readonly Regex RxSep123 = new(@"[.\-_](?<ver>\d+(?:\.\d+){0,2})\b", RegexOptions.CultureInvariant);
    private static readonly Regex RxTrailingInt = new(@"(?<ver>\d+)\b", RegexOptions.CultureInvariant);

    private sealed class Manifest
    {
        public string Culture { get; set; } = "";
        public string Version { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
        public string Path { get; set; } = "";
    }

    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // A parancssori verzió elsőbbséget élvez a fájlnévből felismert értékkel szemben.
        var versionArg = ParseVersionArg(args);

        int created = 0, errors = 0;

        foreach (var culture in Cultures)
        {
            try
            {
                var inputDir = Path.Combine(InputRoot, culture);
                var outputDir = Path.Combine(OutputRoot, culture);

                if (!Directory.Exists(inputDir))
                {
                    Console.WriteLine($"[INFO] No input directory for '{culture}' → skip.");
                    continue;
                }

                var candidate = Directory.EnumerateFiles(inputDir, "*.html", SearchOption.TopDirectoryOnly)
                                         .OrderByDescending(f => new FileInfo(f).LastWriteTimeUtc)
                                         .FirstOrDefault();

                if (candidate is null)
                {
                    Console.WriteLine($"[INFO] No HTML in {inputDir} → skip '{culture}'.");
                    continue;
                }

                var version = !string.IsNullOrWhiteSpace(versionArg)
                              ? versionArg
                              : ExtractVersionFromFileName(Path.GetFileNameWithoutExtension(candidate));

                if (string.IsNullOrWhiteSpace(version))
                    throw new InvalidOperationException(
                        $"No version provided and none found in '{Path.GetFileName(candidate)}'. " +
                        "Provide --version X.Y or include a version in the filename (e.g., aszfv1.1.html).");

                Directory.CreateDirectory(outputDir);

                var outputFileName = $"{OutputBaseName}-v{version}.html";
                var outputFilePath = Path.Combine(outputDir, outputFileName);

                File.Copy(candidate, outputFilePath, overwrite: true);

                var manifest = new Manifest
                {
                    Culture = culture,
                    Version = version,
                    UpdatedAt = DateTime.UtcNow,
                    Path = $"/legal/{culture}/{outputFileName}"
                };

                var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Path.Combine(outputDir, "manifest.json"), manifestJson, Encoding.UTF8);

                Console.WriteLine($"[OK] {culture}: {outputFileName} + manifest.json");
                created++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {culture}: {ex.Message}");
                errors++;
            }
        }

        Console.WriteLine($"\nSummary: created={created}, errors={errors}");
        return errors == 0 ? 0 : 1;
    }

    private static string ParseVersionArg(string[] args)
    {
        // Mind a „--version 1.1”, mind a „--version=1.1” alak támogatott.
        for (int i = 0; i < args.Length; i++)
        {
            var a = args[i];
            if (a.Equals("--version", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length) return NormalizeVersion(args[i + 1]);
                return "";
            }

            if (a.StartsWith("--version=", StringComparison.OrdinalIgnoreCase))
            {
                var val = a[(a.IndexOf('=') + 1)..];
                return NormalizeVersion(val);
            }
        }
        return "";
    }

    private static string NormalizeVersion(string? v)
    {
        if (string.IsNullOrWhiteSpace(v)) return "";
        v = v.Trim();
        if (v.StartsWith('v') || v.StartsWith('V')) v = v[1..];
        return v;
    }

    private static string ExtractVersionFromFileName(string name)
    {
        // Az első illeszkedő verzióminta határozza meg az eredményt.
        // aszfv1.1, aszf.v1.1, aszf-v1.1, aszf_1.1, terms2, stb.
        var m = RxV123.Match(name);
        if (m.Success) return m.Groups["ver"].Value;

        m = RxSep123.Match(name);
        if (m.Success) return m.Groups["ver"].Value;

        // Utolsó minta: név végén álló egész szám
        var tail = RxTrailingInt.Matches(name).LastOrDefault();
        if (tail is not null) return tail.Groups["ver"].Value;

        return "";
    }
}
