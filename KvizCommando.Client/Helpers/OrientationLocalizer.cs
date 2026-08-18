using KvizCommando.Client.Data;
using KvizCommando.Client.Utilities;
using KvizCommando.Server.Data.StaticData;

namespace KvizCommando.Client.Helpers
{
    public class OrientationLocalizer
    {
        public static string GetOrientation(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture); ;
            var row = OrientationNameTable.Data[index];
            return lang switch
            {
                "hu" => row.NameHu,
                "en" => row.NameEn,
                //"de" => row.NameDe,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }

        public static string GetOrientShort(int index, string culture)
        {
            var lang = LocationNormalizer.CultFormat(culture);
            var row = OrientationNameTable.Data[index];
            return lang switch
            {
                "hu" => row.ShortHu,
                "en" => row.ShortEn,
                //"de" => row.ShortDe,
                _ => throw new ArgumentOutOfRangeException(nameof(lang))
            };
        }
    }
}
