
using KvizCommando.Server.Data.StaticData;
using System.Collections.Generic;

namespace KvizCommando.Client.Services.Visual
{
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
        /// <inheritdoc />
        public IReadOnlyList<CategoryOption> GetAll()  => _options;
        /// <inheritdoc />
        public string ResolveLabel(int code, string culture)
        {
            for (var i = 0; i < _options.Count; i++)
            {
                
                if (_options[i].Code == code) return culture switch
                {
                    "hu" => _options[i].LabelHu,
                    "en" => _options[i].LabelEn,
                    _ => throw new ArgumentOutOfRangeException(nameof(culture))
                }; 
            }
            return $"Ismeretlen ({code})";
        }

        /// <inheritdoc />
        public bool TryResolveLabel(int code, out string label, string culture)
        {
            for (var i = 0; i < _options.Count; i++)
            {
                if (_options[i].Code == code)
                {
                    label = culture switch
                    {
                        "hu" => _options[i].LabelHu,
                        "en" => _options[i].LabelEn,
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
