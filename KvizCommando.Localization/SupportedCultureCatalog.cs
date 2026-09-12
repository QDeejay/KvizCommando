using System.Globalization;

namespace KvizCommando.Localization;

public static class SupportedCultureCatalog
{
    public const string DefaultCultureName = "hu-HU";

    private static readonly IReadOnlyDictionary<string, CultureInfo> Cultures =
        new[]
        {
            "hu-HU",
            "en-US"
        }
        .Select(CultureInfo.GetCultureInfo)
        .ToDictionary(
            culture => culture.Name,
            StringComparer.OrdinalIgnoreCase);

    public static IEnumerable<CultureInfo> All => Cultures.Values;

    public static CultureInfo Resolve(params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (TryGet(candidate, out var culture))
                return culture;
        }

        return Cultures[DefaultCultureName];
    }

    public static bool TryGet(string? cultureName, out CultureInfo culture)
    {
        culture = default!;

        if (string.IsNullOrWhiteSpace(cultureName))
            return false;

        try
        {
            var normalizedName = CultureInfo.GetCultureInfo(cultureName).Name;

            return Cultures.TryGetValue(normalizedName, out culture!);
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }
}
