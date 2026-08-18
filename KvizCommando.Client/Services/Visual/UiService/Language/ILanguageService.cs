namespace KvizCommando.Client.Services.Visual.UiService.Language
{
    public interface ILanguageService
    {
        string this[string key] { get; }
        /// <summary>
        /// Betölti a megadott lokalizációs modult.
        /// </summary>
        /// <param name="culture">A kért kultúra neve, például <c>hu-HU</c>.</param>
        /// <param name="moduleName">A betöltendő lokalizációs modul neve.</param>
        Task LoadModuleAsync(string culture, string moduleName);
        /// <summary>
        /// Törli a megadott nyelvhez tartozó lokalizációs gyorsítótárat.
        /// </summary>
        /// <param name="culture">A kért kultúra neve, például <c>hu-HU</c>.</param>
        Task ClearLanguageCacheAsync(string culture);
      
    }
}
