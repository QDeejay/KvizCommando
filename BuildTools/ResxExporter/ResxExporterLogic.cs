using Newtonsoft.Json;
using System.Collections;
using System.ComponentModel.Design;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Text;

namespace ResxExporter;

public static class ResxExporterLogic
{
    public static bool ProcessResx(string resxRoot, string outputRoot, Action<string> logInfo, Action<string> logWarning, Action<string> logError)
    {
        if (!Directory.Exists(resxRoot))
        {
            logError($"[ResxExporter] Input folder not found: {resxRoot}");
            return false;
        }

        var resxFiles = Directory.GetFiles(resxRoot, "*.resx");
        var cultureGroups = resxFiles
            .Select(f => new { Path = f, File = Path.GetFileNameWithoutExtension(f) })
            .Where(x => x.File.Contains('.'))
            .GroupBy(x => x.File.Split('.').Last());

        foreach (var group in cultureGroups)
        {
            string culture = group.Key;
            string outputCultureDir = Path.Combine(outputRoot, culture);
            string manifestPath = Path.Combine(resxRoot, "localization", culture, "manifest.json");
            string outputManifestPath = Path.Combine(outputCultureDir, "manifest.json");

            Directory.CreateDirectory(outputCultureDir);

            Dictionary<string, string> previousManifest = new();
            if (File.Exists(manifestPath))
            {
                try
                {
                    previousManifest = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(manifestPath)) ?? new();
                }
                catch (Exception ex)
                {
                    logWarning($"[ResxExporter] Could not parse manifest.json: {ex.Message}");
                }
            }

            Dictionary<string, string> newManifest = new();
            Dictionary<string, Dictionary<string, string>> modulesToWrite = new();

            foreach (var resx in group)
            {
                string fullPath = resx.Path;
                string fileName = Path.GetFileNameWithoutExtension(fullPath); // pl.: login.hu
                string module = fileName.Substring(0, fileName.LastIndexOf('.')); // pl.: login

                Dictionary<string, string> flat = ReadResxFile(fullPath, module);
                string json = JsonConvert.SerializeObject(flat, Formatting.Indented);
                string hash = ComputeSHA256(json);
                newManifest[module] = hash;

                if (!previousManifest.TryGetValue(module, out string oldHash) || oldHash != hash)
                {
                    modulesToWrite[module] = flat;
                    logInfo($"[ResxExporter] Changed or new: {culture}/{module}.json");
                }
            }

            bool manifestChanged = !newManifest.OrderBy(kv => kv.Key).SequenceEqual(previousManifest.OrderBy(kv => kv.Key));
            if (modulesToWrite.Count > 0 || manifestChanged)
            {
                foreach (var kv in modulesToWrite)
                {
                    string filePath = Path.Combine(outputCultureDir, kv.Key + ".json");
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(kv.Value, Formatting.Indented));
                }

                Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
                File.WriteAllText(manifestPath, JsonConvert.SerializeObject(newManifest, Formatting.Indented));
                File.Copy(manifestPath, outputManifestPath, true);
                logInfo($"[ResxExporter] Exported manifest: {culture}/manifest.json");
            }
            else
            {
                logInfo($"[ResxExporter] No changes detected for culture: {culture}");
            }
        }

        return true;
    }
    private static Dictionary<string, string> ReadResxFile(string resxPath, string modulePrefix)
    {
        var result = new Dictionary<string, string>();

        using var reader = new ResXResourceReader(resxPath) { UseResXDataNodes = true };
        foreach (DictionaryEntry entry in reader)
        {
            if (entry.Key is not string key || entry.Value is not ResXDataNode node)
                continue;

            string? value = node.GetValue((ITypeResolutionService)null) as string;

            if (value != null)
            {
                string fullKey = $"{key}";
                result[fullKey] = value;
            }
        }

        return result;
    }


    /*
    private static Dictionary<string, string> ReadResxFile(string resxPath, string modulePrefix)
    {
        Console.WriteLine($"Most ovassa osztaa rühes dzsézont+!");
        var result = new Dictionary<string, string>();
        using var reader = new ResXResourceReader(resxPath) { UseResXDataNodes = true };
        foreach (DictionaryEntry entry in reader)
        {
            if (entry.Key is string key && entry.Value is string value)
            {
                string fullKey = $"{modulePrefix}.{key}";
                Console.WriteLine($"[ResxExporter] Processing key: {fullKey}"); 
                result[fullKey] = value;
                Console.WriteLine($"[ResxExporter] Processing key: {fullKey}");
            }
        }
        return result;
    }
    */
    private static string ComputeSHA256(string content)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
