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

            await ApplyCallbackCultureAsync();
            _culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            await Rules.GetRulesAsync();
            await Ui.Lang.LoadModuleAsync(_culture, "common");
            await Ui.Lang.LoadModuleAsync(_culture, "identityerrors");


            Console.WriteLine("[EmptyLayout] OnInitializedAsync END");

            _isReady = true;
        }

        private async Task ApplyCallbackCultureAsync()
        {
            var uri = Ui.Nav.ToAbsoluteUri(Ui.Nav.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var cultureName = NormalizeCallbackCulture(query["culture"]);

            if (cultureName is null)
                return;

            var culture = new CultureInfo(cultureName);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            await LocalStorage.SetItemAsync("userLang", cultureName);
        }

        private static string? NormalizeCallbackCulture(string? culture)
        {
            if (string.Equals(culture, "hu", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(culture, "hu-HU", StringComparison.OrdinalIgnoreCase))
                return "hu-HU";
            if (string.Equals(culture, "en", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(culture, "en-US", StringComparison.OrdinalIgnoreCase))
                return "en-US";
            return null;
        }
    }
}
