
using KvizCommando.Client.Services.Language;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace KvizCommando.Client.Pages.Shared
{
    public partial class LanguageConfirmBase : ComponentBase 
    {
        [Inject] protected IJSRuntime JS { get; set; } = default!;
        [Inject] private ILanguageService Lang { get; set; } = default!;
        protected string SelectedLanguage { get; set; } = "Placeholder";
        protected string ConfirmMessage { get; set; } = "Placeholder";
        protected string LocalstoreLang { get; set; } = "Placeholder";

        protected string ModalTitle {get; set;} = "Placeholder";

        protected string ReloadText { get; set; } = "Placeholder";
        protected string CancelText { get; set; } = "Placeholder";

        protected async Task ConfirmReload()

        {
            await Lang.ClearLanguageCacheAsync(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            if (SelectedLanguage == "hu")
            {
                LocalstoreLang = "hu-HU";
            }
            else if (SelectedLanguage == "en")
            {
                LocalstoreLang = "en-US";
            }
            else 
            {
                LocalstoreLang = "hu-HU";
            } 



                await JS.InvokeVoidAsync("localStorage.setItem", "userLang", LocalstoreLang);

            await JS.InvokeVoidAsync("location.reload");
        }

        public async void ShowForLanguage(string languageCode, string modaltitle, string message, string reloadText, string cancelText)
        {
            SelectedLanguage = languageCode;
            
            ModalTitle = modaltitle;
            
            ConfirmMessage = message;
            ReloadText = reloadText;
            CancelText = cancelText;
            await InvokeAsync(StateHasChanged);

            // Bootstrap modal megnyitás (interop kell, mert Blazorban nincs direkt show)
            await JS.InvokeVoidAsync("bootstrapModalHelper.show", "#languageConfirmModal");
        }
    }
}
