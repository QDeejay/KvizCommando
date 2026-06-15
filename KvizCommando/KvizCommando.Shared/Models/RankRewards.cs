using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Models
{
    public static class RankRewards
    {
        public static readonly List<RankRewardRow> List = new()
        {
            new(  0,  0,     100,   0, 0, 0, 2, null ),
            new(  1, 10,     300,   2, 1, 2, 3, null ),
            new(  2, 12,     500,   2, 1, 0, 3, 1 ),
            new(  3, 14,     700,   2, 1, 0, 3, 2 ),
            new(  4, 20,    1200,   3, 2, 5, 3, null ),
            new(  5, 22,    1700,   3, 2, 0, 3,null),
            new(  6, 24,    2200,   3, 2, 0, 3,null ),
            new(  7, 30,    3200,   4, 3, 6, 4,null ),
            new(  8, 32,    4200,   4, 3, 0, 4, 3 ),
            new(  9, 34,    5200,   4, 3, 0, 4, 4 ),
            new( 10, 40,    7200,   5, 4, 7, 5, 101 ),
            new( 11, 42,    9200,   5, 4, 0, 5,null),
            new( 12, 44,   11200,   5, 4, 0, 5,null ),
            new( 13, 50,   16200,   6, 5, 8, 6,null ),
            new( 14, 52,   21200,   6, 5, 0, 6,5 ),
            new( 15, 54,   26200,   6, 5, 0, 6,6 ),
            new( 16, 60,   36200,   8, 6, 9, 7,102 ),
            new( 17, 62,   46200,   8, 6, 0, 7,null ),
            new( 18, 64,   56200,   8, 6, 0, 7,null ),
            new( 19, 70,   76200,  10, 7, 10, 8, null ),
            new( 20, 80,   96200,  10, 7, 0, 8, 7 ),
            new( 21,100,  146200,  10, 7, 0, 8, 8 ),
            new( 22,100,     255,  10, 10, 5, 8, 103 ),

            new( 23,100,     255,  10, 10, 5, 8, null ),
            new( 24,100,     255,  10, 10, 5, 8, null ),
            new( 25,100,     255,  10, 10, 5, 8, null ),
            new( 26,100,     255,  10, 10, 5, 8, 104 ),
            new( 27,100,     255,  10, 10, 5, 8, null ),
            new( 28,100,     255,  10, 10, 5, 8, null ),
            new( 29,100,     255,  10, 10, 5, 8, null ),
            new( 30,100,       0,  10, 10, 5, 8, null )
        };
    }
    public class RankRewardRow
    {
        public int RowIndex { get; set; }
        public int WinBonus { get; set; }
        public int NextLevel { get; set; }
        public int OwnQuestSlot { get; set; }
        public int DevPointRevard { get; set; }
        public int DevPointToStore { get; set; }

        public int MaxCharacters { get; set; }

        public int? HelpRewardNo { get; set; }

        public RankRewardRow(
            int rowIndex,
            int winBonus,
            int nextLevel,
            int ownQuestSlot,
            int devPointRevard,
            int devPointToStore,
            int maxCharacters,
            int? helpRewardNo
        )
        {
            RowIndex = rowIndex;
            WinBonus = winBonus;
            NextLevel = nextLevel;
            OwnQuestSlot = ownQuestSlot;
            DevPointRevard = devPointRevard;
            DevPointToStore = devPointToStore;
            MaxCharacters = maxCharacters;
            HelpRewardNo = helpRewardNo;
        }
    }
}
