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
        [Inject] protected ILanguageService Lang { get; set; } = default!;
        [Inject] private IdentityRulesService Rules { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;

        private bool _isReady = false;
        
        protected override async Task OnInitializedAsync()
        {

        Console.WriteLine("[EmptyLayout] OnInitializedAsync START");
            var rules = await Rules.GetRulesAsync();
            var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            await Lang.LoadModuleAsync(culture, "common");
            await Lang.LoadModuleAsync(culture, "identityerrors");

           
            Console.WriteLine("[EmptyLayout] OnInitializedAsync END");
           
            _isReady = true;
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
/*
 
            // var culture = await LocalStorage.GetItemAsync<string>("userLang");
            //if (string.IsNullOrWhiteSpace(culture))
            //{
            //    culture = "hu-HU";
            //    await LocalStorage.SetItemAsync("userLang", culture);
            //}
            //CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
            //CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);
  //Console.WriteLine("[EmptyLayout] Initialized with culture: " + culture);

            // Betöltjük a common modult az aktuális kultúrához
 
  // Vizsgáljuk a URL-t hogy változott e, hogy vissza tudjuk rakni a nyelvválasztót
            // var uri = Nav.ToBaseRelativePath(Nav.Uri).ToLowerInvariant();




            // első indulásnlál ráprobálunk a checkin státuszra, remember me feature miatt
            //var FirstRun = await SessionStorage.GetItemAsync<bool>("FirstRun");
            //if (FirstRun != true)
            // {
            //   await SessionStorage.SetItemAsync("FirstRun", true);
            //   await UserService.CheckInStartAsync(true);
            //}  // Ui.Changed += OnUiChanged;
  //private LanguageConfirmBase? ConfirmModal;
        // protected string culture = "Placeholder";
    //[Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        //[Inject] private ISessionStorageService SessionStorage { get; set; } = default!;
        //[Inject] private IUserService UserService { get; set; } = default!;

//private void OnUiChanged() => InvokeAsync(StateHasChanged);
        //Ui.Changed -= OnUiChanged;
        [Inject] private UiHeaderState Ui { get; set; } = default!;

 */