namespace KvizCommando.Shared.Models
{
    public static class RankRewards
    {
        public static readonly List<RankRewardRow> List =
        [
            new(  0,  0,     100,   100, 0, 0, 0, 2, null ),

            new(  1,  0,     700,   300, 0, 1, 1, 3, 200 ),
            new(  2, 10,    1850,   680, 0, 1, 0, 3, 201 ),
            new(  3, 15,    4200,  1460, 0, 1, 0, 3, null ),

            new(  4, 20,    6850,  2340, 1, 2, 2, 3, 101 ),
            new(  5, 22,   10150,  3440, 1, 2, 0, 3,205),
            new(  6, 24,   15700,  5290, 1, 2, 0, 3,null ),

            new(  7, 30,   21000,  6615, 2, 3, 2, 4,202 ),
            new(  8, 32,   27100,  8140, 2, 3, 0, 4, null ),
            new(  9, 34,   36900,  10590, 2, 3, 0, 4, null ),

            new( 10, 40,   45600,  12330, 3, 4, 3, 5,102  ),
            new( 11, 42,   55100, 14230, 3, 4, 0, 5,null),
            new( 12, 44,   70000, 17210, 3, 4, 0, 5,null ),

            new( 13, 50,   85000, 20210, 4, 5, 4, 6,203 ),
            new( 14, 52,  101000, 23400, 4, 5, 0, 6,null ),
            new( 15, 54,  125800, 28400, 4, 5, 0, 6,null ),

            new( 16, 60,  145100, 32270, 6, 6, 4, 7,103 ),
            new( 17, 62,  165600, 36370, 6, 6, 0, 7,null ),
            new( 18, 64,  197600, 42770, 6, 6, 0, 7,null ),

            new( 19, 70,  225400, 48330, 8, 7, 4, 8, 104 ),
            new( 20, 80,  254600, 54170, 8, 7, 0, 8, null ),
            new( 21, 90,  300000, 60000, 8, 7, 0, 8, null ),

            new( 22,100,     255,  0, 10, 10, 4, 8, 204 ),
            new( 23,100,     255,  0, 10, 10, 4, 8, null ),
            new( 24,100,     255,  0, 10, 10, 4, 8, null ),

            new( 25,100,     255,  0, 10, 10, 4, 8, null ),
            new( 26,100,     255,  0, 10, 10, 4, 8, null ),
            new( 27,100,     255,  0, 10, 10, 4, 8, null ),

            new( 28,100,     255,  0, 10, 10, 4, 8, null ),
            new( 29,100,     255,  0, 10, 10, 4, 8, null ),
            new( 30,100,       0,  0, 10, 10, 4, 8, null )
        ];
    }
    public class RankRewardRow
    {
        public int RowIndex { get; set; }
        public int WinBonus { get; set; }

        public int NextLevelTeam { get; set; }

        public int NextLevelMember { get; set; }
        public int OwnQuestSlot { get; set; }
        public int DevPointRevard { get; set; }
        public int DevPointToStore { get; set; }

        public int MaxCharacters { get; set; }

        public int? HelpRewardNo { get; set; }

        public RankRewardRow(
            int rowIndex,
            int winBonus,
            int nextLevelTeam,
            int nextLevelMember,
            int ownQuestSlot,
            int devPointRevard,
            int devPointToStore,
            int maxCharacters,
            int? helpRewardNo
        )
        {
            RowIndex = rowIndex;
            WinBonus = winBonus;
            NextLevelTeam = nextLevelTeam;
            NextLevelMember = nextLevelMember;
            OwnQuestSlot = ownQuestSlot;
            DevPointRevard = devPointRevard;
            DevPointToStore = devPointToStore;
            MaxCharacters = maxCharacters;
            HelpRewardNo = helpRewardNo;
        }
    }
}
