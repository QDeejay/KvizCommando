namespace KvizCommando.Client.Models.DataModels
{
    public class RankNamesRow
    {
        public int EnumLevel { get; set; }
        public string? PublicLevel { get; set; }
        public string NameHu { get; set; }
        public string ShortHu { get; set; }
        public string NameEn { get; set; }
        public string ShortEn { get; set; }

        public RankNamesRow(
            int enumlevel,
            string publiclevel,
            string namehu,
            string shorthu,
            string nameen,
            string shorten
         )
        {
            EnumLevel = enumlevel;
            PublicLevel = publiclevel;
            NameHu = namehu;
            ShortHu = shorthu;
            NameEn = nameen;
            ShortEn = shorten;
        }
    }

    public class RankClassNameRow
    {
        public int RankClass { get; set; }
        public string RankClassNameHu { get; set; }
        public string RankClassNameEn { get; set; }


        public RankClassNameRow(
             int rankclass,
             string rankclassnamehu,
             string rankclassnameen
         )
        {
            RankClass = rankclass;
            RankClassNameHu = rankclassnamehu;
            RankClassNameEn = rankclassnameen;
        }
    }
    public enum LanguageCode
    {
        hu,
        en,
        de
    }
}
