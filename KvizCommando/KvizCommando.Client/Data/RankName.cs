using KvizCommando.Client.Models.DataModels;

namespace KvizCommando.Client.Data  
{
    public static class RankNameTable
    {
        public static readonly List<RankNamesRow> Data =
        [
            new(0,  "Lvl-NA",              "Közlegény", "kk.",                  "Private", "PVT"),
            new(1,  "Lvl-00",               "Örvezető", "őrv.",               "Private 2", "PV2"),
            new(2,  "Lvl-01",                "Tizedes", "tiz.",              "Private FC", "PFC"),
            new(3,  "Lvl-02",          "Szakaszvezető", "szkv.",               "Corporal", "CPL"),
            new(4,  "Lvl-03",               "Őrmester", "őrm.",                 "Segeant", "SGT"),
            new(5,  "Lvl-04",          "Törzsőrmester", "tőrm.",         "Staff Sergeant", "SSG"),
            new(6,  "Lvl-05",        "Főtőrgyőrmester", "ftőrm.",  "Sergeant First class", "SFC"),
            new(7,  "Lvl-06",                "Zászlós", "zls.",         "Warrant Officer", "WO1"),
            new(8,  "Lvl-07",           "Tőrzszászlós", "tzls.",        "Chief Warrant 2", "CW2"),
            new(9,  "Lvl-08",         "Főtörgyzászlós", "ftzls.",       "Chief Warrant 3", "CW3"),
            new(10, "Lvl-09",                "Hadnagy", "hdgy.",        "Second Lieutant", "2LT"),
            new(11, "Lvl-10",              "Főhadnagy", "fhdgy.",              "Lieutant", "1LT"),
            new(12, "Lvl-11",               "Százados", "szds.",                "Captain", "CPT"),
            new(13, "Lvl-12",                 "Őrnagy", "őrgy.",                 "Major", "MAJ"),
            new(14, "Lvl-13",              "Alezredes", "alez.",     "Lieutenant Colonel", "LTC"),
            new(15, "Lvl-14",                "Ezredes", "ezds.",                "Colonel", "COL"),
            new(16, "Lvl-15",         "Dandártábornok", "ddtbk.",     "Brigadier General", "BG"),
            new(17, "Lvl-16",            "Vezérörnagy", "vőrgy.",         "Major General", "MG"),
            new(18, "Lvl-17",            "Altábornagy", "altbgy.",   "Lieutenant General", "LTG"),
            new(19, "Lvl-18",           "Vezérezredes", "vezds.",              "General ", "GEN"),
            new(20, "Lvl-19",       "Hadseregtábornok", "ht.",      "General of the army", "GOA"),
            new(21, "Lvl-20",   "Hadseregek tábornoka", "hkt.",   "General of the armies", "GAS"),
            new(22, "Lvl-MX",   "Honvédelmi miniszter", "hvm.",     "Minister of defense", "MOD"),
            new(23, "Lvl-MX1",    "Minisztérium vezető", "miv.",     "Head of ministerium", "HOM"),
            new(24, "Lvl-MX2",    "Kvíz Miniszterelnök", "kme.",      "Quiz Primeminister", "PRM"),
            new(25, "Lvl-MX3",             "Kvíz elnök", "ke.",            "Quiz president", "QP"),
            new(26, "Lvl-MX4",    "Kontinentális elnök", "kke.",   "Continental president", "COP"),
            new(27, "Lvl-MX5",   "Kvízek bolygó elnöke", "bke.",    "Planet prez. of Quiz", "POQ"),
            new(28, "Lvl-MX6",      "Kvízek védangyala", "kva.",           "Angel of Quiz", "AOQ"),
            new(29, "Lvl-MX7",       "Kvízek istensége", "kvi.",             "God of Quiz", "GOQ"),
            new(30, "Lvl-MX8","Kvízek Mészáros Lőrince", "ml.", "Lawrence Butcher of Quiz", "LBQ"),

            new(31, "Lvl-MX",   "Nyugalmazott veterán", "nyv.",         "Retired veteran", "REV")
        ];
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
            new(1,  "Járőr",               "Patrol"),
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
