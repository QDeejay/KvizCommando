using KvizCommando.Server.Models;
using KvizCommando.Shared.Models;
using System.Text.Json;

namespace KvizCommando.Server.Utilities.Recruit
{

    public static class RecruitService
    {

        public static RecruitSlot Generate(int count, int expDays)
        {
            return new RecruitSlot
            {
                Names = GenerateNames(count),
                PictureCodes = GeneratePicCodes(count),
                ExpirationTime = DateTime.UtcNow.AddDays(expDays)
            };
        }
        public static (int[], int[], int[]) RecruitResolver(int member, int candidate)
        {
            string m = RecruitData.OrientKeys[member - 1];

            bool c = candidate == 1 || candidate == 3 || candidate == 5 || candidate == 7 ? true : false;
            candidate--;
            bool[] mask = RecruitData.RecruitMask[candidate / 2];
            int[] idx =
            {
                int.Parse(m[0].ToString()),
                int.Parse(m[1].ToString()),
                int.Parse(m[2].ToString()),
                int.Parse(m[3].ToString()),
                int.Parse(m[4].ToString()),
                int.Parse(m[5].ToString()),
                int.Parse(m[6].ToString()),
                int.Parse(m[7].ToString()),
            };

            return (
                new int[]
                {
                    idx[0] + (mask[0] ? 8:0),
                    idx[1] + (mask[1] ? 8:0),
                    idx[0] + (mask[2] ? 8:0),
                    idx[1] + (mask[3] ? 8:0)},
                new int[]
                {
                    (c ? idx[2] : idx[4]) + (mask[0] ? 8:0),
                    (c ? idx[3] : idx[5]) + (mask[1] ? 8:0),
                    (c ? idx[2] : idx[4]) + (mask[2] ? 8:0),
                    (c ? idx[3] : idx[5]) + (mask[3] ? 8:0)
                },
                new int[]
                {
                    idx[6] + (mask[4] ? 8:0),
                    idx[6] + (mask[5] ? 8:0),
                    idx[7] + (mask[6] ? 8:0),
                    idx[7] + (mask[7] ? 8:0)
                });
        }

        private static string[] GenerateNames(int count)
        {
            string[] result = new string[count];
            var maleData = Load("Data/NameGeneratorData/names_male.json");
            var femaleData = Load("Data/NameGeneratorData/names_female.json");
            var lastData = Load("Data/NameGeneratorData/names_last.json");

            for (int i = 0; i < count; i++)
            {
                bool isMale = i < count / 2;

                string first = isMale
                    ? maleData[Random.Shared.Next(maleData.Length)]
                    : femaleData[Random.Shared.Next(femaleData.Length)];

                string last = lastData[Random.Shared.Next(lastData.Length)];

                result[i] = $"{first} {last}";
            }

            return result;
        }
        private static string[] Load(string file)
        {
            var json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<string[]>(json)
                   ?? [];
        }
        private static string[] GeneratePicCodes(int count)
        {
            string[] result = new string[count];

            for (int i = 0; i < count; i++)
                result[i] = GenerateOne(i < count / 2);
            return result;
        }
        private static string GenerateOne(bool sex)
        {
            var rnd = Random.Shared;
            char genre = sex ? 'M' : 'F'; ;
            int skin = rnd.Next(0, 8);
            int eye = skin > 3 ? rnd.Next(2, 4) : rnd.Next(0, 4);
            int hair = (skin > 3 && genre != 'F') ? rnd.Next(4, 8) : rnd.Next(0, 8);
            int hairStyle = genre == 'F' ? rnd.Next(1, 8) : rnd.Next(0, 8);
            char asian = (skin < 4 && rnd.Next(0, 100) < 25) ? 'A' : 'B';
            char glasses = rnd.Next(0, 100) < 25 ? 'G' : 'N';

            return $"{genre}{skin}{eye}{hair}{hairStyle}{asian}{glasses}";
        }

    }

}

/*
 *  : IRecruitService
 * 
 * 
   public interface IRecruitService
    {
        RecruitSlot Generate(int count);
        (int[], int[], int[]) RecruitResolver(int member, int candidate);
    }

    private readonly INameGenerator _names;
        private readonly IPicCodeGenerator _pics;



 public RecruitService(INameGenerator names, IPicCodeGenerator pics)
        {
            _names = names;
            _pics = pics;
        }

 */