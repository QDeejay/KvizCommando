namespace KvizCommando.Client.Services.Visual.UiService.Language
{
    public interface ILanguageService
    {
        string this[string key] { get; }
        /// <summary>
        /// Betölti a megadott lokalizációs modult.
        /// </summary>
        Task LoadModuleAsync(string culture, string moduleName);
        /// <summary>
        /// Törli a megadott nyelvhez tartozó lokalizációs gyorsítótárat.
        /// </summary>
        Task ClearLanguageCacheAsync(string culture);
      
    }
}
