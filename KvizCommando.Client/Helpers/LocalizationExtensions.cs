namespace KvizCommando.Client.Helpers
{
    public static class LocalizationExtensions
    {
        /// <summary>
        /// A formázási helyőrzőket a hiányzó argumentumok mellett is biztonságosan behelyettesíti.
        /// </summary>
        public static string FormatSafe(this string template, params object[] args)
        {
            if (string.IsNullOrEmpty(template) || args == null || args.Length == 0)
                return template;

            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                // Hibás formátum esetén az eredeti sablon biztonságosabb, mint egy renderelési kivétel.
                return TryPartialReplace(template, args);
            }
        }
        private static string TryPartialReplace(string template, object[] args)
        {
            string result = template;

            for (int i = 0; i < args.Length; i++)
            {
                string pattern = $"{{{i}}}";
                result = result.Replace(pattern, args[i]?.ToString() ?? string.Empty);
            }

            return result;
        }
    }
}
