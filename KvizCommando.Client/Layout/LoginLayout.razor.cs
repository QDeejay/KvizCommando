using Blazored.LocalStorage;
using KvizCommando.Client.Services;
using KvizCommando.Client.Utilities;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Layout
{
    public partial class LoginLayout : KcLayoutComponentBase
    {
        [Inject] private IdentityRulesService Rules { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;

        private string _culture = "hu";
        private bool _isReady = false;

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("[EmptyLayout] OnInitializedAsync START");

            _culture = await InitCultureAsync();
            await Rules.GetRulesAsync();
            await Ui.Lang.LoadModuleAsync(_culture, "common");
            await Ui.Lang.LoadModuleAsync(_culture, "identityerrors");


            Console.WriteLine("[EmptyLayout] OnInitializedAsync END");

            _isReady = true;
        }

        private async Task<string> InitCultureAsync()
        {
            var uri = Ui.Nav.ToAbsoluteUri(Ui.Nav.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var cultureName = query["culture"];

            if (string.IsNullOrWhiteSpace(cultureName))
                cultureName = await LocalStorage.GetItemAsync<string>("userLang");

            if (string.IsNullOrWhiteSpace(cultureName))
                cultureName = "hu-HU";

            var culture = new CultureInfo(cultureName);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            await LocalStorage.SetItemAsync("userLang", culture.Name);

            return culture.TwoLetterISOLanguageName;
        }
    }
}
