using KvizCommando.Client.Services.Visual.UiService.Language;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Components
{
    public partial class LangGuard
    {
        [Inject] private ILanguageService Lang { get; set; } = default!;
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
    }
}
