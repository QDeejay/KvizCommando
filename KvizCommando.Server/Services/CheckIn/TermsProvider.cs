#nullable enable
using System.Globalization;
using System.Text.Json;
using KvizCommando.Shared.Contracts.CheckIn;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace KvizCommando.Server.Services.CheckIn
{
    /// <summary>
    /// Fájl-alapú (wwwroot/legal/{culture}/manifest.json) Terms provider rövid memóriacache-sel.
    /// - Minden kultúrára külön cache entry.
    /// - Cache érvénytelenítés: rövid TTL + file change token (ha a fájl változik).
    /// - GDPR-minimum: csak metaadatot szolgáltat (Version/ETag, URL, PublishedAt).
    /// </summary>
    public sealed class TermsProvider : ITermsProvider
    {
        private readonly string _webRoot;
        private readonly IFileProvider _webRootProvider;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TermsProvider> _logger;

        // Rövid, de kényelmes élettartam. Ha aktívan szerkeszted a fájlt, a ChangeToken azonnal érvénytelenít.
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(45);

        public TermsProvider(IWebHostEnvironment env, IMemoryCache cache, ILogger<TermsProvider> logger)
        {
            _webRoot = string.IsNullOrWhiteSpace(env.WebRootPath)
                ? Path.Combine(AppContext.BaseDirectory, "wwwroot")
                : env.WebRootPath;

            _webRootProvider = env.WebRootFileProvider ?? new PhysicalFileProvider(_webRoot);
            _cache = cache;
            _logger = logger;
        }

        public string CurrentTermsEtag => GetCurrentTerms().Version;

        public TermsMeta GetCurrentTerms()
        {
            var culture = NormalizeCulture(CultureInfo.CurrentUICulture?.Name ?? CultureInfo.CurrentCulture.Name);
            var cacheKey = $"terms:{culture}";

            if (_cache.TryGetValue(cacheKey, out TermsMeta cached) && cached is not null)
                return cached;

            var pathRelative = $"legal/{culture}/manifest.json";
            var fileInfo = _webRootProvider.GetFileInfo(pathRelative);

            TermsMeta meta;

            if (fileInfo.Exists)
            {
                try
                {
                    using var stream = fileInfo.CreateReadStream();
                    var dto = JsonSerializer.Deserialize<ManifestDto>(stream, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (dto is null)
                    {
                        _logger.LogWarning("Terms manifest deserialization returned null. culture={Culture}", culture);
                        meta = Fallback();
                    }
                    else
                    {
                        meta = new TermsMeta
                        {
                            Version = string.IsNullOrWhiteSpace(dto.Version) ? "unknown" : dto.Version!,
                            Url = NormalizeUrl(dto.Path),
                            PublishedAt = ParseUtc(dto.UpdatedAt)
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read Terms manifest. culture={Culture}", culture);
                    meta = Fallback();
                }
            }
            else
            {
                _logger.LogWarning("Terms manifest file not found at {Path}. culture={Culture}", pathRelative, culture);
                meta = Fallback();
            }

            // Cache beállítása: rövid TTL + file change token (ha van)
            var changeToken = _webRootProvider.Watch(pathRelative); // mindig ad IChangeToken-t, még ha nem is létezik
            _cache.Set(cacheKey, meta, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheTtl)
                .AddExpirationToken(changeToken));

            return meta;
        }

        public bool IsValidVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version)) return false;
            var current = GetCurrentTerms();
            return string.Equals(current.Version, version, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeCulture(string culture)
        {
            if (string.IsNullOrWhiteSpace(culture)) return "hu-HU";
            culture = culture.Trim();
            if (culture.Equals("hu", StringComparison.OrdinalIgnoreCase)) return "hu-HU";
            if (culture.Equals("en", StringComparison.OrdinalIgnoreCase)) return "en-US";
            return culture;
        }

        private static DateTime ParseUtc(string? updatedAt)
        {
            if (string.IsNullOrWhiteSpace(updatedAt)) return DateTime.UtcNow;

            if (DateTime.TryParse(
                    updatedAt,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dt))
            {
                return dt;
            }

            return DateTime.UtcNow;
        }

        private static string NormalizeUrl(string? path)
            => string.IsNullOrWhiteSpace(path) ? "/legal/terms-missing.html"
                                               : (path.StartsWith("/") ? path : "/" + path);

        private static TermsMeta Fallback() => new TermsMeta
        {
            Version = "unknown",
            Url = "/legal/terms-missing.html",
            PublishedAt = DateTime.UtcNow
        };

        private sealed class ManifestDto
        {
            public string? Culture { get; set; }
            public string? Version { get; set; }
            public string? UpdatedAt { get; set; }
            public string? Path { get; set; }
        }
    }
}
