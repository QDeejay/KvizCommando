using KvizCommando.Client.Data;
using KvizCommando.Client.Utilities;

namespace KvizCommando.Client.Helpers

{
    public class RankNameLocalizer
    {
        public static string GetName(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture); ;
            var row = RankNameTable.Data[index];
            return lang switch
            {
                "hu" => row.NameHu,
                "en" => row.NameEn,
                //"de" => row.NameDe,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }

        public static string GetShort(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture);
            var row = RankNameTable.Data[index];
            return lang switch
            {
                "hu" => row.ShortHu,
                "en" => row.ShortEn,
                //"de" => row.ShortDe,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }
        public static string GetClass(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture);
            var row = RankClassNameTable.Data[index];
            return lang switch
            {
                "hu" => row.RankClassNameHu,
                "en" => row.RankClassNameEn,
                //"de" => row.RankClassNameDe,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }
    }
}
