using KvizCommando.Client.Models.DataModels;

namespace KvizCommando.Server.Data.StaticData
{
    public static class CategoryTable
    {


        public static readonly Dictionary<int, CategoryRow> Data = new()
        {
            [0] = new(0, "Összes", "All", "<--->", "<--->"),
            [1] = new(1, "Vallás", "Religion", "<Val>", "<Rel>"),
            [2] = new(2, "Dátumok és idő", "Dates & Time", "<Dat>", "<Dat>"),
            [3] = new(3, "Zene", "Music", "<Zen>", "<Mus>"),
            [4] = new(4, "Sport", "Sport", "<Spo>", "<Spo>"),
            [5] = new(5, "Technologia", "Technology", "<Tec>", "<Tec>"),
            [6] = new(6, "Természet tudományok", "Natural science", "<Ter>", "<Nat>"),
            [7] = new(7, "Híres emberek", "Famous people", "<H.E>", "<F.P>"),
            [8] = new(8, "Képzőművészet", "Fine arts", "<Kép>", "<FiA>"),
            [9] = new(9, "Mitologia", "Míthology", "<Mit>", "<Myt>"),
            [10] = new(10, "Történelem", "History", "<Tör>", "<His>"),
            [11] = new(11, "Filmek", "Movies", "<Fil>", "<Mov>"),
            [12] = new(12, "Játék", "Games", "<Ját>", "<Gam>"),
            [13] = new(13, "Informatika", "IT", "<Inf>", "<IT.>"),
            [14] = new(14, "Földrajz-Csillagászat", "Geography-Astronomy", "<FCS>", "<G.A>"),
            [15] = new(15, "Divat és márkák", "Fashion & brands", "<Div>", "<Fas>"),
            [16] = new(16, "Irodalom", "Literature", "<Iro>", "<Lit>"),
            [17] = new(17, "Saját", "Own", "<Iro>", "<Lit>"),
            [99] = new(99, "Tipp", "Guess", "", ""),
            [101] = new(101, "50-50", "50-50", "", ""),
            [102] = new(102, "Tipp sáv", "Guess snipe", "", ""),
            [103] = new(103, "Időtlenítő", "Time reset", "", ""),
            [104] = new(104, "AI javaslat", "AI suggest", "", ""),
            [200] = new(200, "Csapat orvos", "Team doctor", "", ""),
            [201] = new(201, "Új csapatnév", "New team name", "", ""),
            [202] = new(202, "Felderítő", "Scout", "", ""),
            [203] = new(203, "Raktáros", "Storekeeper", "", ""),
            [204] = new(204, "Motivátor", "Motivator", "", ""),
        };

    }
}
