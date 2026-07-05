using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Models
{
    public static class RankConstants
    {
        public static readonly int[] startLevels =   // skillek és helpek start levelje
        [
           0,0,0,0,2, 3, 8, 9, 14, 15, 20, 21,10,16,22,26
        ];
        public static readonly int[] maxLevels = // skillek és helpek max levelje
        [
            21,21,21,21,20,20,14,14,8,8,2,2,1,15,1,5 
        ];
    }
}
