using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Client.Utilities;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Components
{
    public partial class LangGuard : IDisposable
    {
        [Inject] private ILanguageService Lang { get; set; } = default!;
        //[Inject] private ISetLanguageService SetLang { get; set; } = default!;
        [Parameter] public required string Culture { get; set; }
        [Parameter] public string Module { get; set; } = default!;
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public RenderFragment? LoadingContent { get; set; }
        [Parameter] public EventCallback OnReady { get; set; }

        private bool _isReady = false;
        private string? _lastCulture;
        private string prevModule = string.Empty;
        protected override async Task OnParametersSetAsync()
        {
            if (prevModule != Module)
            { 
                _isReady = false;
                await EnsureModuleLoadedAsync();
                prevModule = Module;
            }
            
        }

        protected override void OnInitialized()
        {
            // Feliratkozás a nyelvváltás eseményre
            //SetLang.LanguageChanged += OnLanguageChanged;
        }

        private async void OnLanguageChanged()
        {
            // Nyelvváltáskor újra töltjük a modult az aktuális kultúrával
            _isReady = false;
            await EnsureModuleLoadedAsync();
            await InvokeAsync(StateHasChanged);
        }

        private async Task EnsureModuleLoadedAsync()
        {
          

            if (_lastCulture != Culture || !_isReady)
            {
                if (_lastCulture != null && _lastCulture != Culture)
                {
                    await Lang.ClearLanguageCacheAsync(_lastCulture);
                }

                _lastCulture = Culture;
                await Lang.LoadModuleAsync(Culture, Module);
                await OnReady.InvokeAsync();
                _isReady = true;
            }

        }

        public void Dispose()
        {
            // SetLang.LanguageChanged -= OnLanguageChanged;
            GC.SuppressFinalize(this);
        }
    }
}
