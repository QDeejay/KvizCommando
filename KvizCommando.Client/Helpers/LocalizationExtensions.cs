namespace KvizCommando.Client.Helpers
{
    public static class LocalizationExtensions
    {
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
                // Visszaadjuk az eredeti stringet – vagy megpróbáljuk részlegesen behelyettesíteni
                return TryPartialReplace(template, args);
            }
        }
        // Részleges helyettesítés megkísérlése, ha a formázás sikertelen
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
