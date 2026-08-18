using KvizCommando.Client.Data;
using KvizCommando.Client.Utilities;
using KvizCommando.Server.Data.StaticData;

namespace KvizCommando.Client.Helpers
{
    public class CategoryNameLocalizer
    {
        public static string GetCategory(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture); ;
            var row = CategoryTable.Data[index];
            return lang switch
            {
                "hu" => row.CategoryHu,
                "en" => row.CategoryEn,
                //"de" => row.CategoryEn,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }

        public static string GetCatShort(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture);
            var row = CategoryTable.Data[index];
            return lang switch
            {
                "hu" => row.ShortCatHu,
                "en" => row.ShortCatEn,
                //"de" => row.ShortCatDe,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }
    }
}
