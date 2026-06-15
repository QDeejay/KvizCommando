using Blazored.LocalStorage;
using Blazored.SessionStorage;
using KvizCommando.Client.Pages.Shared;
using KvizCommando.Client.Services;
using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace KvizCommando.Client.Layout
{
    public partial class EmptyLayout : LayoutComponentBase, IDisposable
    {
        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] private ISessionStorageService SessionStorage { get; set; } = default!;
        [Inject] private IUserService UserService { get; set; } = default!;
        [Inject] private IdentityRulesService Rules { get; set; } = default!;
        [Inject] private ILoadingService Loader { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private UiHeaderState Ui { get; set; } = default!;
     
        protected bool HideLanguageSelector => Ui.HideLanguageSelector;

        public bool DisableLanguageSelector { get; set; }

        private LanguageConfirmBase? ConfirmModal;
        protected string culture = "Placeholder";
        private bool _isReady = false;
        
        protected override async Task OnInitializedAsync()
        {
           
            Console.WriteLine("[EmptyLayout] OnInitializedAsync START");

            var culture = await LocalStorage.GetItemAsync<string>("userLang");
            if (string.IsNullOrWhiteSpace(culture))
            {
                culture = "hu-HU";
                await LocalStorage.SetItemAsync("userLang", culture);
            }
           

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);
            var rules = await Rules.GetRulesAsync();
            culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            Console.WriteLine("[EmptyLayout] Initialized with culture: " + culture);

            // Betöltjük a common modult az aktuális kultúrához
            await Lang.LoadModuleAsync(culture, "common");
            await Lang.LoadModuleAsync(culture, "identityerrors");
            Console.WriteLine("[EmptyLayout] Loaded module 'common' for " + culture);

            // Vizsgáljuk a URL-t hogy változott e, hogy vissza tudjuk rakni a nyelvválasztót
            var uri = Nav.ToBaseRelativePath(Nav.Uri).ToLowerInvariant();
            Ui.Changed += OnUiChanged;

            Console.WriteLine("[EmptyLayout] OnInitializedAsync END");

            // első indulásnlál ráprobálunk a checkin státuszra, remember me feature miatt
            var FirstRun = await SessionStorage.GetItemAsync<bool>("FirstRun");
            if (FirstRun != true)
            {
                await SessionStorage.SetItemAsync("FirstRun", true);
                await UserService.CheckInStartAsync(true);
            }
            
            await Loader.Hide();
            _isReady = true;
        }
        private void OnUiChanged() => InvokeAsync(StateHasChanged);
        private void HuClick() => ShowConfirm("hu");
        private void EnClick() => ShowConfirm("en");
        private void ShowConfirm(string lang)
        {
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName != lang )
            { 
            ConfirmModal?.ShowForLanguage(
                    lang,
                    Lang[$"common.Modal.Language.Title.{lang}"],
                    Lang[$"common.Modal.Language.Content.{lang}"],
                    Lang[$"common.Modal.Language.Restart.{lang}"],
                    Lang["common.Modal.Language.Keep"]
                );
            }
        }
        public void Dispose()
        {
            Ui.Changed -= OnUiChanged;
            GC.SuppressFinalize(this);
        }
    }
}
