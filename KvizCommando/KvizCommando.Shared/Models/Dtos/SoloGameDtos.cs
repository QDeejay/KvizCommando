using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //public ResultDto OverallCategory { get; set; } = default!;
            //public ResultDto OverallOrient { get; set; } = default!;
     }
    public sealed class ResultDto
    {
        public int Points { get; set; } = 0;
        public double Time { get; set; } = 0.0;
    }


}


/*
 
    public sealed class SoloGameDtos
    {
        public readonly bool[] InitEna = [false, false, false, false, false, false, false, false];

        public bool[] ActiveOris = new bool[8];

        public Results Results { get; set; } = default!;
       
    }
    public class Results 
    {
        public ResultDtos[] CategoryResults { get; set; } = new ResultDtos[17];
        public ResultDtos[] OrientResults { get; set; } = new ResultDtos[9];
        public int CategoryOverall => CategoryResults?.Sum(r => r?.Points ?? 0) ?? 0;
        public int OreintOverall => OrientResults?.Sum(r => r?.Points ?? 0) ?? 0;
    }
    public sealed class ResultDtos
    {
        public int Points { get; set; } = 0;
        public double Time { get; set; } = 0.0;
    }
    public sealed class SoloButtonEnables(bool[] mask) {


        public bool GameCampaignEna { get; set; } = mask.Any(x => x);
        public bool GameCategoryEna { get; set; } = mask.Any(x => x);
        public bool GameOrientsEna { get; set; } = false;
        public bool[] OrientEna => mask;
        public bool[] CatEna => mask is null ? Array.Empty<bool>() : mask.Concat(mask).ToArray();
    }

 
 public sealed class SoloGameDtos
    {
        // Raw adatok - referencia, 0 allokáció
        public ResultDto[] CategoryResults { get;  }
        public ResultDto[] OrientResults { get; }

        // Enable maskok - 1 allokáció csak a CatEna
        public bool[] OrientEna { get; }
        public bool[] CatEna { get; }

        // Összesített értékek - előre számolva
        public int CategoryOverall { get; }
        public int OrientOverall { get; }
        public double CategoryTimeOverall { get; }
        public double OrientTimeOverall { get; }

        // Gomb enable flag-ek - előre számolva
        public bool GameCampaignEna { get; }
        public bool GameCategoryEna { get; }
        public bool GameOrientsEna { get; }

        public SoloGameDtos(bool campaign,bool[] mask, ResultDto[] orientResults, ResultDto[] categoryResults)
        {
            // 1. Validálás nélkül a leggyorsabb, de productionbe kell
            ArgumentNullException.ThrowIfNull(mask);
            ArgumentNullException.ThrowIfNull(orientResults);
            ArgumentNullException.ThrowIfNull(categoryResults);

            // 2. Referenciák átvétele - 0 allokáció
            OrientEna = mask;
            OrientResults = orientResults;
            CategoryResults = categoryResults;

            // 3. CatEna 1x allokáció - mask duplázva
            CatEna = new bool[mask.Length << 1]; // * 2, bitshift gyorsabb
            mask.CopyTo(CatEna, 0);
            mask.CopyTo(CatEna, mask.Length);

            // 4. Összegek 1x loop, nincs LINQ allokáció
            int catPoints = 0, oriPoints = 0;
            double catTime = 0, oriTime = 0;

            for (int i = 0; i < categoryResults.Length; i++)
            {
                var r = categoryResults[i];
                catPoints += r.Points;
                catTime += r.Time;
            }

            for (int i = 0; i < orientResults.Length; i++)
            {
                var r = orientResults[i];
                oriPoints += r.Points;
                oriTime += r.Time;
            }

            CategoryOverall = catPoints;
            OrientOverall = oriPoints;
            CategoryTimeOverall = catTime;
            OrientTimeOverall = oriTime;

            // 5. Enable flag-ek - Span.Contains gyorsabb mint Any()
            var anyEnabled = mask.AsSpan().Contains(true);
            GameCampaignEna = campaign;
            GameCategoryEna = anyEnabled;
            GameOrientsEna = anyEnabled;
        }
    }

 */