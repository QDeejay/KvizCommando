namespace KvizCommando.Client.Utilities
{

    public class LocationNormalizer
    {
        /// <summary>
        /// Egységes alkalmazási formára alakítja a kultúra azonosítóját.
        /// </summary>
        public static string CultFormat(string? culture)
        {
            if (string.IsNullOrWhiteSpace(culture)) return "en";
            return culture.Length >= 2
                ? culture[..2].ToLowerInvariant()
                : culture.ToLowerInvariant();
        }
    }
        
}
