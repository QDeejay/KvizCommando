using KvizCommando.Client.Data;
using KvizCommando.Client.Utilities;

namespace KvizCommando.Client.Helpers

{
    public class RankNameLocalizer
    {
        /// <summary>
        /// Visszaadja a rendfokozat lokalizált nevét.
        /// </summary>
        public static string GetName(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture); ;
            var row = RankNameTable.Data[index];
            return lang switch
            {
                "hu" => row.NameHu,
                "en" => row.NameEn,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }

        /// <summary>
        /// Visszaadja a rendfokozat lokalizált rövid nevét.
        /// </summary>
        public static string GetShort(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture);
            var row = RankNameTable.Data[index];
            return lang switch
            {
                "hu" => row.ShortHu,
                "en" => row.ShortEn,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }
        /// <summary>
        /// Visszaadja a rendfokozati osztály lokalizált nevét.
        /// </summary>
        public static string GetClass(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture);
            var row = RankClassNameTable.Data[index];
            return lang switch
            {
                "hu" => row.RankClassNameHu,
                "en" => row.RankClassNameEn,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }
        /// <summary>
        /// Visszaadja a csapatszint lokalizált nevét.
        /// </summary>
        public static string GetTeam(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture);
            var row = TeamRankNameTable.Data[index];
            return lang switch
            {
                "hu" => row.TeamHu,
                "en" => row.TeamEn,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }

    }
}
