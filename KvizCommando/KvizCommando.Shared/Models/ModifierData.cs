using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Models
{
    public static class ModifierTable
    {
        public static readonly List<ModifierTableRow> Data = new()
        {
            new(0, -1.0, 8.0, -1.0, 7.0, -0.5, 5.0, -0.5, 4.0, 0,         100, 0,    0),
            new(1, -1.4, 7.7, -1.3, 6.5, -0.9, 4.5, -1.2, 2.5, 1,        50, 1,   65),
            new(2, -1.7, 7.4, -1.7, 6.0, -1.3, 4.0, -2.0, 1.0, null,     47, null,70),
            new(3, -2.1, 7.1, -2.1, 5.5, -1.7, 3.5, null, null,null,     44, null,75),
            new(4, -2.4, 6.8, -2.5, 5.1, -2.1, 3.0, null, null,null,     41,null, 85),
            new(5, -2.7, 6.5, -2.9, 4.7, -2.5, 2.5, null, null,null,     38, null,90),
            new(6, -3.1, 6.2, -3.3, 4.4, -3.0, 2.0, null, null,null,     35, null,null),
            new(7, -3.5, 5.9, -3.7, 4.1, -3.5, 1.5, null, null,null,     32,null,null),
            new(8, -3.8, 5.6, -4.0, 3.8, -4.0, 1.0, null, null,null,     29, null,null ),
            new(9, -4.2, 5.3, -4.4, 3.5, null, null, null, null,null,    26,null, null),
            new(10, -4.6, 5.0, -4.7, 3.2, null, null, null, null,null,   23,null, null),
            new(11, -5.0, 4.7, -5.0, 2.8, null, null, null, null,null,   20, null,null),
            new(12, -5.3, 4.4, -5.3, 2.4, null, null, null, null,null,   18,null, null),
            new(13, -5.6, 4.1, -5.6, 2.0, null, null, null, null,null,   16, null,null),
            new(14, -6.0, 3.8, -6.0, 1.5, null, null, null, null,null,   14, null,null),
            new(15, -6.3, 3.5, null, null, null, null, null, null,null,  12,null, null),
            new(16, -6.6, 3.2, null, null, null, null, null, null,null,  10, null,null),
            new(17, -7.0, 2.9, null, null, null, null, null, null,null,   8,null, null),
            new(18, -7.3, 2.6, null, null, null, null, null, null,null, null, null,null),
            new(19, -7.6, 2.3, null, null, null, null, null, null,null, null, null,null),
            new(20, -8.0, 2.0, null, null, null, null, null, null,null, null, null,null)
        };
        public static readonly List<MainModifiersRow> DataMainSkill = new()
        {
            new(0, -3, -0.3),
            new(1,  9, -0.3),
            new(2, -2, -0.3),
            new(3,  8, -0.3),
        };
    }
    public class ModifierTableRow
    {
        public int RowIndex { get; set; }
        public double?[] Modifier { get; set; } = new double?[12];


        public ModifierTableRow(
            int rowIndex,
            params double?[] modifer)
        {
            RowIndex = rowIndex;
            Modifier = modifer;
        }
    }
    public class MainModifiersRow
    { 
        public int Index { get; set; }
        public int StartValue { get; set; }
        public double StepValue { get; set; }

        public MainModifiersRow(
            int index,
            int startvalue,
            double stepvalue) 
        {
            Index = index;
            StartValue = startvalue;
            StepValue = stepvalue;
        }

    }
}
