using KvizCommando.Client.Models.DataModels;
using KvizCommando.Shared.Models.Ranks;

namespace KvizCommando.Client.Data
{
    public static class RankNameTable
    {
        public static readonly IReadOnlyList<RankDefinition> Data = RankCatalog.Data;
    }

    public static class RankClassNameTable
    {
        public static readonly List<RankClassNameRow> Data =
        [
            new(0, "Rang nélkül", "No Rank"),
            new(1, "Tisztesek", "Enlisted"),
            new(2, "Altisztek", "Non-Commissioned Officers"),
            new(3, "Zászlósok", "Warrant Officers"),
            new(4, "Tisztek", "Officers"),
            new(5, "Főtisztek", "Senior Officers"),
            new(6, "Tábornokok", "Generals"),
            new(7, "Főtábornokok","Head Generals"),
            new(8, "Miniszterek","Ministers"),
            new(9, "Elnökök","Presidents"),
            new(10, "Istenségek","Gods"),
            new(11, "Nyugdíjas klub","Retired club")
        ];
    }

    public static class TeamRankNameTable
    {
        public static readonly List<TeamNameRow> Data =
        [
            new(0,  "Újonc Csapat",        "Recruit Team"),
            new(1,  "Járőr Csapat",        "Patrol Team"),
            new(2,  "Tűzcsoport",          "Fire Team"),
            new(3,  "Raj",                 "Squad"),
            new(4,  "Erősített Raj",       "Heavy Squad"),
            new(5,  "Harci Raj",           "Combat Squad"),
            new(6,  "Különítmény",         "Detachment"),
            new(7,  "Szakasz",             "Platoon"),
            new(8,  "Harci Szakasz",       "Combat Platoon"),
            new(9,  "Bevetési Egység",     "Task Force"),
            new(10, "Század",              "Company"),
            new(11, "Harci Század",        "Combat Company"),
            new(12, "Elit Század",         "Elite Company" ),
            new(13, "Zászlóalj",           "Battalion" ),
            new(14, "Harci Zászlóalj",     "Combat Battalion"),
            new(15, "Ezred",               "Regiment"),
            new(16, "Dandár",              "Brigade"),
            new(17, "Hadosztály",          "Division"),
            new(18, "Hadtest",             "Corps"),
            new(19, "Hadsereg",            "Army"),
            new(20, "Hadseregcsoport",     "Army Group"),
            new(21, "Főparancsnokság",     "High Command"),

            new(22, "Kvíz Minisztérium",   "Quiz Ministry"),
            new(23, "Kvíz Kormány",        "Quiz Government"),
            new(24, "Kvíz Állam",          "Quiz State"),
            new(25, "Kvíz Köztársaság",    "Quiz Republic"),
            new(26, "Kontinentális Unió",  "Continental Union"),
            new(27, "Bolygó Szövetség",    "Planetary Alliance"),
            new(28, "Galaktikus Birodalom","Galactic Empire"),
            new(29, "Kvíz Pantheon",       "Quiz Pantheon"),
            new(30, "Kvíz Multiverzum",    "Quiz Multiverse"),

            new(31, "Kvíz Multiverzum",    "Quiz Multiverse"),
        ];
    }

}
