
using CsvHelper.Configuration.Attributes;
using KvizCommando.Server.Data.StaticData;
using System.Collections.Generic;

namespace KvizCommando.Client.Services.Visual
{
    // Ezt a statikus szolgáltatást nyugodtan lecserélheted a CSV-alapú implementációdra.
    public sealed class StaticCategoryLookupService : ICategoryLookupService
    {

        private static List<CategoryOption> _options => GetFromDict();
        
        private static List<CategoryOption> GetFromDict()
        { 
            var opts = new List<CategoryOption>();
            
            for (int i = 0; i < 18; i++)
            {
                opts.Add(new CategoryOption(i, CategoryTable.Data[i].CategoryHu, CategoryTable.Data[i].CategoryEn));
            }
            return opts;
        }
        public IReadOnlyList<CategoryOption> GetAll()  => _options;
        /*{
            
            var co = new List<CategoryOption>();
            for (int i = 0; i < 18; i++)
            {
                co.Add(new CategoryOption(i, CategoryTable.Data[i].CategoryHu, CategoryTable.Data[i].CategoryEn ));
            }
            return co;
        }
        */
        public string ResolveLabel(int code, string culture)
        {
            // Nincs FirstOrDefault: kézi keresés
            for (var i = 0; i < _options.Count; i++)
            {
                
                if (_options[i].Code == code) return culture switch
                {
                    "hu" => _options[i].LabelHu,
                    "en" => _options[i].LabelEn,
                    //"de" => row.ShortDe,
                    _ => throw new ArgumentOutOfRangeException(nameof(culture))
                }; 
            }
            return $"Ismeretlen ({code})";
        }

        public bool TryResolveLabel(int code, out string label, string culture)
        {
            for (var i = 0; i < _options.Count; i++)
            {
                if (_options[i].Code == code)
                {
                    //label = culture == "hu" ? _options[i].LabelHu : _options[i].LabelEn; 
                    label = culture switch
                    {
                        "hu" => _options[i].LabelHu,
                        "en" => _options[i].LabelEn,
                        //"de" => row.ShortDe,
                        _ => throw new ArgumentOutOfRangeException(nameof(culture))
                    };
                    return true;
                }
            }
            label = string.Empty;
            return false;
        }
    }
}
