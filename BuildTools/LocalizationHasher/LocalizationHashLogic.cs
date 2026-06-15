using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LocalizationHasher
{
    public static class LocalizationHashLogic
    {
        public static bool ProcessRoot(string sourceRoot, string targetRoot, Action<string> logInfo, Action<string> logWarning, Action<string> logError)
        {
            if (!Directory.Exists(sourceRoot))
            {
                logError($"[LocalizationHasher] Input directory '{sourceRoot}' not found.");
                return false;
            }

            foreach (var cultureDir in Directory.GetDirectories(sourceRoot))
            {
                string culture = Path.GetFileName(cultureDir);
                string manifestPath = Path.Combine(cultureDir, "manifest.json");
                var currentHashes = new Dictionary<string, string>();

                if (File.Exists(manifestPath))
                {
                    try
                    {
                        var existing = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(manifestPath));
                        if (existing != null)
                            currentHashes = existing;
                    }
                    catch (Exception e)
                    {
                        logWarning($"[LocalizationHasher] Failed to read previous manifest for {culture}: {e.Message}");
                    }
                }

                var newHashes = new Dictionary<string, string>();
                bool hasChanged = false;

                foreach (var file in Directory.GetFiles(cultureDir, "*.json"))
                {
                    if (Path.GetFileName(file).Equals("manifest.json", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string hash = ComputeSHA256(file);
                    string module = Path.GetFileNameWithoutExtension(file);
                    newHashes[module] = hash;

                    if (!currentHashes.TryGetValue(module, out var oldHash) || oldHash != hash)
                    {
                        hasChanged = true;
                    }
                }

                string destCultureDir = Path.Combine(targetRoot, culture);
                bool targetMissingOrEmpty = !Directory.Exists(destCultureDir) || Directory.GetFiles(destCultureDir, "*.json").Length == 0;

                if (hasChanged || currentHashes.Count != newHashes.Count || targetMissingOrEmpty)
                {
                    File.WriteAllText(manifestPath, JsonSerializer.Serialize(newHashes, new JsonSerializerOptions { WriteIndented = true }));
                    logInfo($"[LocalizationHasher] Updated manifest: {manifestPath}");

                    // Másolás a célkönyvtárba
                    //destCultureDir = Path.Combine(targetRoot, culture);
                    Directory.CreateDirectory(destCultureDir);
                    string destManifestPath = Path.Combine(destCultureDir, "manifest.json");
                    File.Copy(manifestPath, destManifestPath, overwrite: true);
                    logInfo($"[LocalizationHasher] Copied manifest to: {destManifestPath}");

                    foreach (var kvp in newHashes)
                    {
                        string sourceFile = Path.Combine(cultureDir, kvp.Key + ".json");
                        string destFile = Path.Combine(destCultureDir, kvp.Key + ".json");

                        try
                        {
                            File.Copy(sourceFile, destFile, overwrite: true);
                            logInfo($"[LocalizationHasher] Copied: {culture}/{kvp.Key}.json");
                        }
                        catch (Exception ex)
                        {
                            logWarning($"[LocalizationHasher] Failed to copy '{kvp.Key}.json': {ex.Message}");
                        }
                    }
                }
                else
                {
                    logInfo($"[LocalizationHasher] No changes for culture: {culture}");
                }
            }

            return true;
        }

        private static string ComputeSHA256(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(stream);
            return BytesToHex(hashBytes);
        }

        private static string BytesToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
