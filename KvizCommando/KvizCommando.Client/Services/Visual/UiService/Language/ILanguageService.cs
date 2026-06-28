namespace KvizCommando.Client.Services.Visual.UiService.Language
{
    public interface ILanguageService
    {
        string this[string key] { get; }
        Task LoadModuleAsync(string culture, string moduleName);
        Task ClearLanguageCacheAsync(string culture);
      
    }
}
