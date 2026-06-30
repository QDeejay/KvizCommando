using Blazored.LocalStorage;
using Blazored.SessionStorage;
using KvizCommando.Client.Pages.Shared;
using KvizCommando.Client.Services;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace KvizCommando.Client.Layout
{
    public partial class LoginLayout : KcLayoutComponentBase, IDisposable
    {
        //[Inject] private ILanguageService _lang { get; set; } = default!;
        [Inject] private IdentityRulesService Rules { get; set; } = default!;
        private readonly string _culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private bool _isReady = false;
       
        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("[EmptyLayout] OnInitializedAsync START");

            var rules = await Rules.GetRulesAsync();
            await Ui.Lang.LoadModuleAsync(_culture, "common");
            await Ui.Lang.LoadModuleAsync(_culture, "identityerrors");
            
           
            Console.WriteLine("[EmptyLayout] OnInitializedAsync END");
           
            _isReady = true;
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}