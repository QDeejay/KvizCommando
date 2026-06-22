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
    public partial class LoginLayout : LayoutComponentBase, IDisposable
    {
        [Inject] private ILanguageService _lang { get; set; } = default!;
        [Inject] private IdentityRulesService _rules { get; set; } = default!;

        private bool _isReady = false;
        private string _culture => CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("[EmptyLayout] OnInitializedAsync START");

            var rules = await _rules.GetRulesAsync();
           
            await _lang.LoadModuleAsync(_culture, "common");
            await _lang.LoadModuleAsync(_culture, "identityerrors");

           
            Console.WriteLine("[EmptyLayout] OnInitializedAsync END");
           
            _isReady = true;
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}