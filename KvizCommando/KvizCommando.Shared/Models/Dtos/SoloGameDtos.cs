using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Models.Dtos
{
    public sealed class SoloGameDtos
    {
        public bool[] ActiveOrients { get; set; } = new bool[8];
      
        public ResultDtos[] CategoryResults { get; set; } = new ResultDtos[17];
        public ResultDtos[] OrientResults { get; set; } = new ResultDtos[9];
        //public int CategoryOverall => CategoryResults?.Sum(r => r?.Points ?? 0) ?? 0;
        //public int OreintOverall => OrientResults?.Sum(r => r?.Points ?? 0) ?? 0;

        //public bool[] ActiveCats => ActiveOrients is null  ? Array.Empty<bool>() : ActiveOrients.Concat(ActiveOrients).ToArray();
    }

    public sealed class ResultDtos
    {
        public int Points { get; set; } = 0;
        public double Time { get; set; } = 0.0;
    }

}
