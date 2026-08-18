using KvizCommando.Client.Helpers;
using Microsoft.JSInterop;
using System.Text.Json;

namespace KvizCommando.Client.Services.Visual.UiService.Language
{
    public class LanguageService : ILanguageService
    {
        private readonly IJSRuntime _js;
        private readonly HttpClient _http;
        private readonly Dictionary<string, string> _translations = new();
        private readonly HashSet<string> _loadedModules = new();
        public string CurrentCulture { get; private set; } = string.Empty;
        public bool IsReady => _loadedModules.Count > 0;
        
        public LanguageService(IJSRuntime js, HttpClient http)
        {
            _js = js;
            _http = http;
        }
        public string this[string key] => Get(key);
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            return _translations.TryGetValue(key, out var value) ? value : $"#{key}";
        }
        public string GetFormatted(string key, params object[] args)
        {
            var template = Get(key);
            return template.FormatSafe(args);
        }
        public async Task LoadModuleAsync(string culture, string moduleName)
        {
            if (_loadedModules.Contains(moduleName))
                return;

            string cacheKey = $"langcache.{culture}.{moduleName}";

            // Először az adott böngészőfül session cache-ét használjuk.
            string? cachedJson = await _js.InvokeAsync<string?>("sessionStorage.getItem", cacheKey);
            bool hasJson = !string.IsNullOrWhiteSpace(cachedJson) && cachedJson.TrimStart().StartsWith("{");

            if (hasJson)
            {
                var moduleTranslations = JsonSerializer.Deserialize<Dictionary<string, string>>(cachedJson!)!;
                foreach (var kv in moduleTranslations) _translations[kv.Key] = kv.Value;
                _loadedModules.Add(moduleName);
                CurrentCulture = culture;
                return;
            }

            // Cache miss esetén a statikus JSON válasza no-cache fejlécet kap a szervertől.
            await _js.InvokeVoidAsync("sessionStorage.removeItem", cacheKey);

            string moduleUrl = $"localization/{culture}/{moduleName}.json";
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

            foreach (var kv in freshModule) _translations[kv.Key] = kv.Value;
            _loadedModules.Add(moduleName);
            CurrentCulture = culture;
        }


        public async Task ClearLanguageCacheAsync(string deleteculture)
        {
            Console.WriteLine($"--- Törlés indul: {deleteculture}");

            // A korábbi nyelv betöltött moduljainak session cache-e törlődik.
            foreach (var module in _loadedModules.ToArray())
            {
                await _js.InvokeVoidAsync("sessionStorage.removeItem", $"langcache.{deleteculture}.{module}");
            }

            // Memóriabeli állapot nullázása a következő nyelv betöltése előtt.
            _loadedModules.Clear();
            _translations.Clear();
            CurrentCulture = string.Empty;
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

    }
}
