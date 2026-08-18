using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Models
{
    public class RecruitData
    {

        public static readonly string[] OrientKeys = {
            "16354872",
            "27163584",
            "35274861",
            "48162753",
            "54718236",
            "63548217",
            "71546328",
            "82637145"
        };
        public static readonly bool[][] RecruitMask = {
            [ false, false, true, true, false, true, true, false ],
            [ true, true, false, false, false, true, true, false ],
            [ false, false, true, true, true, false, false, true ],
            [ true, true, false, false, true, false, false, true ]
        };

    }
}
