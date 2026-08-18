using System;

namespace KvizCommando.Shared.Models.Dtos
{


    public sealed class SoloGameDtos
    {
        public bool AccessDenied { get; set; } = false;
        public bool[] Mask { get; set; } = [];

        public SoloEnables Enables { get; set; } = default!;
        public SoloResults Results { get; set; } = default!;
        
    }
   
    public sealed class SoloEnables
    {
        public bool EnaCampaign { get; set; } = false;
        public bool EnaCategory { get; set; } = false;
        public bool EnaOrient { get; set; } = false;

        public bool[] EnaCat { get; set; } = new bool[16];
        public bool[] EnaOri { get; set; } = new bool[8];
    }
    public sealed class SoloResults 
    {
            public ResultDto[] OrientResults { get; set; } = [];
            public ResultDto[] CategoryResults { get; set; } = [];
     }
    public sealed class ResultDto
    {
        public int Points { get; set; } = 0;
        public double Time { get; set; } = 0.0;
        public string TimeStr { get; set; } = string.Empty;
    }


}
