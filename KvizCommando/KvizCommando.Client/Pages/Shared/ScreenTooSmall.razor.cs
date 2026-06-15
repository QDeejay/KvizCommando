using KvizCommando.Client.Services.Language;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KvizCommando.Client.Pages
{
    public partial class ScreenTooSmallBase : ComponentBase, IAsyncDisposable
    {
        [Inject] protected IJSRuntime JS { get; set; } = default!;
        [Inject] protected ILanguageService Lang { get; set; } = default!;

        protected bool ShowWarning { get; set; }


        private DotNetObjectReference<ScreenTooSmallBase>? _objRef;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _objRef = DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("screenWatcher.init", _objRef);
            }
        }

        [JSInvokable]
        public void UpdateScreenState(int width, int height)
        {
            ShowWarning = width < 1000 || height < 700;
            StateHasChanged();
        }

        public async ValueTask DisposeAsync()
        {
            if (_objRef is not null)
                await JS.InvokeVoidAsync("screenWatcher.cleanup");

            _objRef?.Dispose();
        }
    }
}
