using KvizCommando.Client.Data;
using KvizCommando.Client.Utilities;
using KvizCommando.Server.Data.StaticData;

namespace KvizCommando.Client.Helpers
{
    public class OrientationLocalizer
    {
        /// <summary>
        /// Visszaadja az orientáció lokalizált nevét.
        /// </summary>
        public static string GetOrientation(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture); ;
            var row = OrientationNameTable.Data[index];
            return lang switch
            {
                "hu" => row.NameHu,
                "en" => row.NameEn,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }

        /// <summary>
        /// Visszaadja az orientáció lokalizált rövid nevét.
        /// </summary>
        public static string GetOrientShort(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture);
            var row = OrientationNameTable.Data[index];
            return lang switch
            {
                "hu" => row.ShortHu,
                "en" => row.ShortEn,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }
    }
}
