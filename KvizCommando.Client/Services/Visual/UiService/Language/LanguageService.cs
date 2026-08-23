using KvizCommando.Client.Helpers;
using Blazored.SessionStorage;
using System.Text.Json;

namespace KvizCommando.Client.Services.Visual.UiService.Language
{
    public class LanguageService : ILanguageService
    {
        private readonly ISessionStorageService _sessionStorage;
        private readonly HttpClient _http;
        private readonly Dictionary<string, string> _translations = new();
        private readonly HashSet<string> _loadedModules = new();
        public string CurrentCulture { get; private set; } = string.Empty;
        public bool IsReady => _loadedModules.Count > 0;
        
        public LanguageService(
            ISessionStorageService sessionStorage,
            HttpClient http)
        {
            _sessionStorage = sessionStorage;
            _http = http;
        }
        public string this[string key] => Get(key);
        /// <summary>
        /// Visszaadja a lokalizációs kulcshoz tartozó szöveget.
        /// </summary>
        /// <param name="key">A feloldandó lokalizációs kulcs.</param>
        /// <returns>A lokalizált szöveg; hiányzó kulcsnál a kulcs <c>#</c> előtaggal.</returns>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            return _translations.TryGetValue(key, out var value) ? value : $"#{key}";
        }
        /// <summary>
        /// Visszaadja a lokalizált és paraméterekkel formázott szöveget.
        /// </summary>
        /// <param name="key">A feloldandó lokalizációs kulcs.</param>
        /// <param name="args">A formátumszöveg helyőrzőibe kerülő értékek.</param>
        public string GetFormatted(string key, params object[] args)
        {
            var template = Get(key);
            return template.FormatSafe(args);
        }
        /// <inheritdoc />
        public async Task LoadModuleAsync(string culture, string moduleName)
        {
            if (_loadedModules.Contains(moduleName))
                return;

            string cacheKey = $"langcache.{culture}.{moduleName}";

            // Először az adott böngészőfül session cache-ét használjuk.
            var cachedModule = await _sessionStorage
                .GetItemAsync<Dictionary<string, string>>(cacheKey);
            if (cachedModule is not null)
            {
                foreach (var kv in cachedModule) _translations[kv.Key] = kv.Value;
                _loadedModules.Add(moduleName);
                CurrentCulture = culture;
                return;
            }

            // Cache miss esetén a statikus JSON válasza no-cache fejlécet kap a szervertől.
            await _sessionStorage.RemoveItemAsync(cacheKey);

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

            await _sessionStorage.SetItemAsync(cacheKey, freshModule);

            foreach (var kv in freshModule) _translations[kv.Key] = kv.Value;
            _loadedModules.Add(moduleName);
            CurrentCulture = culture;
        }


        /// <inheritdoc />
        public async Task ClearLanguageCacheAsync(string deleteculture)
        {
            Console.WriteLine($"--- Törlés indul: {deleteculture}");

            // A korábbi nyelv betöltött moduljainak session cache-e törlődik.
            foreach (var module in _loadedModules.ToArray())
            {
                await _sessionStorage.RemoveItemAsync(
                    $"langcache.{deleteculture}.{module}");
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
