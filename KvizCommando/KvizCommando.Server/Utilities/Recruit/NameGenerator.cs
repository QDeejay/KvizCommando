using System.Text.Json;

namespace KvizCommando.Server.Utilities.Recruit
{
    public interface INameGenerator
    {
        string[] GenerateNames(int count);
    }

    public class NameGenerator : INameGenerator
    {
        private readonly string[] _male;
        private readonly string[] _female;
        private readonly string[] _last;

        public NameGenerator()
        {
            _male = Load("Data/NameGeneratorData/names_male.json");
            _female = Load("Data/NameGeneratorData/names_female.json");
            _last = Load("Data/NameGeneratorData/names_last.json");
        }

        private string[] Load(string file)
        {
            var json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<string[]>(json)
                   ?? Array.Empty<string>();
        }

        public string[] GenerateNames(int count)
        {
            string[] result = new string[count];

            for (int i = 0; i < count; i++)
            {
                bool isMale = i<count/2;

                string first = isMale
                    ? _male[Random.Shared.Next(_male.Length)]
                    : _female[Random.Shared.Next(_female.Length)];

                string last = _last[Random.Shared.Next(_last.Length)];

                result[i] = $"{first} {last}";
            }

            return result;
        }
    }

}
