using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Contracts.Team
{
    public class ModifySkillRequest
    {
        public int SkillType { get; set; } = 0;
        public int MemberId { get; set; } = 0;  
        public int[] SkillChanges { get; set; } = new int[4];

    }
}
