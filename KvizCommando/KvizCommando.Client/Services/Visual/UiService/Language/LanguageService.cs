using KvizCommando.Client.Helpers;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace KvizCommando.Client.Services.Visual.UiService.Language
{
    public class LanguageService : ILanguageService
    {
        private readonly IJSRuntime _js;
        private readonly HttpClient _http;
        private readonly Dictionary<string, string> _translations = new();
        private readonly HashSet<string> _loadedModules = new();
        private Dictionary<string, string>? _manifest;
        public string CurrentCulture { get; private set; }
        public bool IsReady => _loadedModules.Count > 0;
        
        public LanguageService(IJSRuntime js, HttpClient http)
        {
            _js = js;
            _http = http;
            Console.WriteLine($"[DI] LanguageService created: {GetHashCode()}");

        }
        public string this[string key] => Get(key);
        public string Get(string key)
        {
            return _translations.TryGetValue(key, out var value) ? value : $"#{key}";
        }
        public string GetFormatted(string key, params object[] args)
        {
            var template = Get(key);
            return template.FormatSafe(args);
        }
        public async Task LoadModuleAsync(string culture, string moduleName)
        {
            await EnsureManifestAsync(culture);
            if (_manifest is null || !_manifest.TryGetValue(moduleName, out var expectedHash) || string.IsNullOrWhiteSpace(expectedHash))
            {
                Console.WriteLine($"[Lang] Manifest missing hash for {moduleName}");
                return;
            }

            if (_loadedModules.Contains(moduleName))
                return;

            string cacheKey = $"langcache.{culture}.{moduleName}";
            string hashKey = $"langhash.{culture}.{moduleName}";

            // próbáljuk a sessionStorage cache-t
            string? cachedJson = await _js.InvokeAsync<string?>("sessionStorage.getItem", cacheKey);
            string? cachedHash = await _js.InvokeAsync<string?>("sessionStorage.getItem", hashKey);

            bool hasJson = !string.IsNullOrWhiteSpace(cachedJson) && cachedJson.TrimStart().StartsWith("{");
            bool hashMatches = !string.IsNullOrWhiteSpace(cachedHash) && cachedHash == expectedHash;

            if (hasJson && hashMatches)
            {
                var moduleTranslations = JsonSerializer.Deserialize<Dictionary<string, string>>(cachedJson!)!;
                foreach (var kv in moduleTranslations) _translations[kv.Key] = kv.Value;
                _loadedModules.Add(moduleName);
                return;
            }

            // cache miss → töltsd le a modult
            await _js.InvokeVoidAsync("sessionStorage.removeItem", cacheKey);
            await _js.InvokeVoidAsync("sessionStorage.removeItem", hashKey);

            // TRÜKK: cache-busting query param a hash-sel
            string moduleUrl = $"localization/{culture}/{moduleName}.json?v={expectedHash}";
            var response = await _http.GetAsync(moduleUrl);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[Lang] ❌ Failed to fetch module: {moduleName}");
                return;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var freshModule = FlattenJson(doc.RootElement, moduleName);

            string serialized = JsonSerializer.Serialize(freshModule);
            await _js.InvokeVoidAsync("sessionStorage.setItem", cacheKey, serialized);
            await _js.InvokeVoidAsync("sessionStorage.setItem", hashKey, expectedHash);

            foreach (var kv in freshModule) _translations[kv.Key] = kv.Value;
            _loadedModules.Add(moduleName);
        }


        public async Task ClearLanguageCacheAsync(string deleteculture)
        {
            Console.WriteLine($"--- Törlés indul: {deleteculture}");

            // 1) modul cache + hash törlése csak a betöltöttekre
            foreach (var module in _loadedModules.ToArray())
            {
                await _js.InvokeVoidAsync("sessionStorage.removeItem", $"langcache.{deleteculture}.{module}");
                await _js.InvokeVoidAsync("sessionStorage.removeItem", $"langhash.{deleteculture}.{module}");
            }

            // 2) manifest törlése az előző nyelvhez
            await _js.InvokeVoidAsync("sessionStorage.removeItem", $"lang.manifest.{deleteculture}");

            // 3) memóriabeli állapot nullázása – fontos, hogy a következő EnsureManifest újra töltsön
            _loadedModules.Clear();
            _translations.Clear();
            _manifest = null;
            CurrentCulture = null;
            Console.WriteLine($"[Lang] Clearing cache for {deleteculture}");

        }

        private static Dictionary<string, string> FlattenJson(JsonElement element, string prefix)
        {
            var result = new Dictionary<string, string>();

            foreach (var prop in element.EnumerateObject())
            {
                var key = $"{prefix}.{prop.Name}";

                if (prop.Value.ValueKind == JsonValueKind.Object)
                {
                    foreach (var sub in FlattenJson(prop.Value, key))
                        result[sub.Key] = sub.Value;
                }
                else if (prop.Value.ValueKind == JsonValueKind.String)
                {
                    result[key] = prop.Value.GetString()!;
                }
            }

            return result;
        }

        private async Task EnsureManifestAsync(string culture)
        {
            if (_manifest != null && CurrentCulture == culture) return;

            // próbáld sessionStorage-ból
            var mk = $"lang.manifest.{culture}";
            var cached = await _js.InvokeAsync<string?>("sessionStorage.getItem", mk);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                _manifest = JsonSerializer.Deserialize<Dictionary<string, string>>(cached!);
                CurrentCulture = culture;
                return;
            }

            // külön API-ról kérd le → nem ragad be a wwwroot cache
            var api = $"/api/lang/manifest?culture={culture}";
            _manifest = await _http.GetFromJsonAsync<Dictionary<string, string>>(api);

            if (_manifest is not null)
            {
                await _js.InvokeVoidAsync("sessionStorage.setItem", mk,
                    JsonSerializer.Serialize(_manifest));
                CurrentCulture = culture;
            }
        }


    }
}

