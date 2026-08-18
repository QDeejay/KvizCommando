using KvizCommando.Client.Services;
using KvizCommando.Client.Utilities;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Layout
{
    public partial class LoginLayout : KcLayoutComponentBase
    {
        [Inject] private IdentityRulesService Rules { get; set; } = default!;
        private readonly string _culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private bool _isReady = false;

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("[EmptyLayout] OnInitializedAsync START");

            await Rules.GetRulesAsync();
            await Ui.Lang.LoadModuleAsync(_culture, "common");
            await Ui.Lang.LoadModuleAsync(_culture, "identityerrors");


            Console.WriteLine("[EmptyLayout] OnInitializedAsync END");

            _isReady = true;
        }
    }
}
