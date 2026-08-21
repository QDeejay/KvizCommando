namespace KvizCommando.Client.Models.DataModels
{
    public sealed class RankClassNameRow
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

    public sealed class TeamNameRow 
    {
        public int EnumLevel { get; set; }
        public string TeamHu { get; set; }
        public string TeamEn { get; set; }

        public TeamNameRow(int enumLevel, string teamHu, string teamEn)
        {
            EnumLevel = enumLevel;
            TeamHu = teamHu;
            TeamEn = teamEn;
        }
    }
}
