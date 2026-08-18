using KvizCommando.Client.Data;
using KvizCommando.Client.Utilities;
using KvizCommando.Server.Data.StaticData;

namespace KvizCommando.Client.Helpers
{
    public class CategoryNameLocalizer
    {
        /// <summary>
        /// Visszaadja a kategória lokalizált nevét.
        /// </summary>
        /// <param name="index">A lokalizálandó érték indexe.</param>
        /// <param name="culture">A kért kultúra neve, például <c>hu-HU</c>.</param>
        public static string GetCategory(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture); ;
            var row = CategoryTable.Data[index];
            return lang switch
            {
                "hu" => row.CategoryHu,
                "en" => row.CategoryEn,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }

        /// <summary>
        /// Visszaadja a kategória lokalizált rövid nevét.
        /// </summary>
        /// <param name="index">A lokalizálandó érték indexe.</param>
        /// <param name="culture">A kért kultúra neve, például <c>hu-HU</c>.</param>
        public static string GetCatShort(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture);
            var row = CategoryTable.Data[index];
            return lang switch
            {
                "hu" => row.ShortCatHu,
                "en" => row.ShortCatEn,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }
    }
}
