using Blazored.LocalStorage;
using KvizCommando.Client.Features.Shared.Modal.Builders;
using KvizCommando.Client.Features.Shared.Modal.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.Visual.UiService;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KvizCommando.Client.Features.Shared.Modal
{
    public partial class KcModal
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] private AudioService Audio { get; set; } = default!;
        [Parameter] public AppState Appstates { get; set; } = default!;
        [Parameter] public string Id { get; set; } = "kcModal";
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public EventCallback OnAction1 { get; set; }
        [Parameter] public EventCallback OnAction2 { get; set; }
        [Parameter] public EventCallback OnCloseAction { get; set; }
        [Parameter] public EventCallback OnCheckBoxAction { get; set; }
        [Parameter] public EventCallback<ModalResult> OnModalAction { get; set; }

        private ModalBoxVm Par = new();
        private bool CanAccept { get; set; } = false;
        private bool _bottomReached;
        private bool CheckBox { get; set; } = false;
        private string? BodyStyle =>
                Par.CheckBottom ? "overflow-y: auto;" : null;

        private string ModalSizeClass =>
            Par.SizeLock ? $"{Par.Size} locked" : Par.Size;
        private sealed record ScrollMetrics(
            double ScrollTop, double ScrollHeight, double ClientHeight,
            double OffsetHeight, double BoxHeight);
        /// <summary>
        /// Megjeleníti a modális ablakot vagy a betöltésjelzőt.
        /// </summary>
        /// <param name="par">A modális ablak tartalmát és működését leíró paraméterek.</param>
        public async Task ShowAsync(ModalBoxVm par)
        {
            Par = par;
            CheckBox = false;
            CanAccept = !Par.CheckBottom;
            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("kcModal.show", $"#{Id}");
            if (Par.CheckBottom == true && _bottomReached != true)
            {

                await Task.Delay(500);
                await CheckBottomAsync();

            }
        }
        /// <summary>
        /// Elrejti az aktuális modális ablakot.
        /// </summary>
        public async Task HideAsync()
        {
            Par = new ModalBoxVm() with { Mode = ModalTypes.None };
            await JS.InvokeVoidAsync("kcModal.hide", $"#{Id}");
            CheckBox = false;
            CanAccept = false;
            _bottomReached = false;
        }
        private async Task OnActionClicked1()
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);

            if (CheckBox == true)
            {
                await LocalStorage.SetItemAsync(Par.CheckBoxKey ?? "ModalChkAction", true);
                if (OnCheckBoxAction.HasDelegate)
                    await OnCheckBoxAction.InvokeAsync();
            }

            if (OnAction1.HasDelegate)
                await OnAction1.InvokeAsync();

            await CompleteAsync(ModalResult.Button1);
        }
        private async Task OnActionClicked2()
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);

            if (OnAction2.HasDelegate)
                await OnAction2.InvokeAsync();

            await CompleteAsync(ModalResult.Button2);
        }
        private async Task CloseAsync()
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);

            if (OnCloseAction.HasDelegate)
                await OnCloseAction.InvokeAsync();

            await CompleteAsync(ModalResult.Close);
        }
        private async Task CompleteAsync(ModalResult result)
        {
            if (OnModalAction.HasDelegate)
                await OnModalAction.InvokeAsync(result);
            else
                await HideAsync();
        }
        private Task OnBodyScroll()
        {
            if (!Par.CheckBottom)
                return Task.CompletedTask;

            return OnTermsScroll();
        }
        private async Task OnTermsScroll()
        {
            if (_bottomReached) return;       // ne pörögjünk feleslegesen
            await CheckBottomAsync();
        }
        private async Task CheckBottomAsync()
        {
            var m = await JS.InvokeAsync<ScrollMetrics?>("kcMeasure", "#termsBody");
            if (m is null) return;

            var atBottom = Math.Ceiling(m.ScrollTop + m.ClientHeight) >= Math.Floor(m.ScrollHeight) - 1;
            if (atBottom)
            {
                _bottomReached = true;
                CanAccept = true;
                StateHasChanged();
            }
        }
    }
}
