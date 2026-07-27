using KvizCommando.Client.Models.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Runtime.Intrinsics.X86;

namespace KvizCommando.Client.Components
{
    public partial class ContentBox : IDisposable
    {
        [Parameter] public ContentBoxVm Vm { get; set; } = default!;
        [Parameter] public EventCallback<int> OnClick { get; set; }
        [Parameter] public EventCallback FooterClick { get; set; }
        [Parameter] public RenderFragment? ContentBody { get; set; }

        private ContentBoxVm _vm = new();
        private bool _noContent = true;
        private string _startSize = string.Empty;

        protected override void OnInitialized()
        {
            _startSize = Vm.Size == "minimalized" ? string.Empty : Vm.Size;
        }

        protected override void OnParametersSet()
        {
            _vm = Vm;
            _noContent = _vm.RenderContent == 0 && string.IsNullOrEmpty(_vm.ImageSrc) && _vm.Size != "content-sized";
        }

        private string CursorStyle { get; set; } = string.Empty;
        private string CssClass =>
            $"kc-content-box {_vm.Size}" + (_vm.IsClickable ? " clickable" : "");
        private string BackgroundStyle =>
            string.IsNullOrWhiteSpace(_vm.BgImageSrc)
                ? ""
                : $"background-image: url('{_vm.BgImageSrc}'); background-size: cover; background-position: center;";
        private string FooterClass =>
            _vm.FooterDisplay ? "kc-content-box--footeractive" : "kc-content-box--footer label-text";
        private async Task HandleClick(MouseEventArgs _)
        {
            if (_vm.IsClickable && _vm.IsEnabled)
            {
                if (OnClick.HasDelegate)
                    await OnClick.InvokeAsync(_vm.ClickId);
            }
        }
        private async Task ToggleSize()
        {
            if (!_vm.ReSizable)
                return;
            if (_vm.Size == "minimalized")
                _vm.Size = _startSize;
            else if (_vm.Size != "minimalized")
            {
                _startSize = _vm.Size;
                _vm.Size = "minimalized";
                if (FooterClick.HasDelegate)
                    await FooterClick.InvokeAsync();
            }
        }
        public void Dispose()
        {
            OnClick = default!;
            FooterClick = default!;
            GC.SuppressFinalize(this);
        }

    }
}