using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Audio;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace KvizCommando.Client.Components
{
    public partial class ContentBox
    {
        [Inject] private AudioService Audio { get; set; } = default!;

        [Parameter] public ContentBoxVm Vm { get; set; } = default!;
        [Parameter] public EventCallback<int> OnClick { get; set; }
        [Parameter] public EventCallback FooterClick { get; set; }
        [Parameter] public RenderFragment? ContentBody { get; set; }

        private ContentBoxVm _vm = new();
        private bool _noContent = true;

        protected override void OnParametersSet()
        {
            _vm = Vm;
            _noContent = _vm.RenderContent == 0 && string.IsNullOrEmpty(_vm.ImageSrc) && _vm.Size != "content-sized";
        }

        private string FooterCursorStyle =>
            _vm.ReSizable
                ? "cursor: url('/Images/cursors/active.cur'), pointer;"
                : string.Empty;
        private string CssClass =>
            $"kc-content-box {_vm.Size}" + (_vm.IsClickable ? " clickable" : "");
        private string BackgroundStyle =>
            string.IsNullOrWhiteSpace(_vm.BgImageSrc)
                ? ""
                : $"background-image:  linear-gradient(var(--kc-content-picture-overlay), var(--kc-content-picture-overlay)), url('{_vm.BgImageSrc}'); background-size: cover; background-position: center;";
        private string BackgroundContentClass =>
            !string.IsNullOrWhiteSpace(_vm.BgImageSrc) &&
            _vm.RenderContent > 0
                ? "background-content"
                : string.Empty;
        private string FooterClass =>
            _vm.FooterDisplay ? "kc-content-box--footeractive kc-lcd-surface" : "kc-content-box--footer label-text";
        private async Task HandleClick(MouseEventArgs _)
        {
            if (_vm.IsClickable && _vm.IsEnabled)
            {
                await Audio.PlaySfxAsync(AudioService.SFX_CLICK);

                if (OnClick.HasDelegate)
                    await OnClick.InvokeAsync(_vm.ClickId);
            }
        }
        private async Task ToggleSize()
        {
            if (!_vm.ReSizable)
                return;
            if (_vm.Size == "minimalized")
                _vm.Size = "large";
            else if (_vm.Size != "minimalized")
            {
                _vm.Size = "minimalized";
                if (FooterClick.HasDelegate)
                    await FooterClick.InvokeAsync();
            }
        }
    }
}
