using KvizCommando.Client.Models.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Drawing;

namespace KvizCommando.Client.Components
{
    public partial class ContentBox : ComponentBase, IDisposable
    {
        [Parameter] public ContentBoxVm Vm { get; set; } = default!;
        //[Parameter] public string Header { get; set; } = "";
        //[Parameter] public string Footer { get; set; } = "";
        //[Parameter] public string Size { get; set; } = "medium"; // small, medium, large, wide
        //[Parameter] public bool IsEnabled { get; set; } = true;
        //[Parameter] public bool IsClickable { get; set; } = false;
        //[Parameter] public bool ShowImage { get; set; } = false;
        //[Parameter] public string? ImageSrc { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public EventCallback<int> OnClick { get; set; }
        [Parameter] public EventCallback FooterClick { get; set; }
        //[Parameter] public bool Resizable { get; set; } = false;
        //[Parameter] public bool FooterDisplay { get; set; } = false;
        //[Parameter] public int ClickId { get; set; }
        //[Parameter] public bool LcdDisplay { get; set; } = false;
        private ContentBoxVm _vm = new ContentBoxVm();
        private bool _noContent = true;
        protected override void OnParametersSet()
        {
            _vm = Vm;
            _noContent = !string.IsNullOrEmpty(_vm.BgImageSrc) && !_vm.ShowImage && string.IsNullOrEmpty(_vm.ImageSrc);
        }
        private string CursorStyle { get; set; } = string.Empty;
        protected string CssClass =>
            $"content-box {_vm.Size}" + (_vm.IsClickable ? " clickable" : "");

        protected string BackgroundStyle =>
            string.IsNullOrWhiteSpace(_vm.BgImageSrc)
                ? ""
                : $"background-image: url('{_vm.BgImageSrc}'); background-size: cover; background-position: center;";

        protected string FooterClass =>
            _vm.FooterDisplay ? "box-footerActive" : "box-footer label-text";
        protected async Task HandleClick(MouseEventArgs _)
        {
            if (_vm.IsClickable && _vm.IsEnabled)
            {
                if(OnClick.HasDelegate)
                    await OnClick.InvokeAsync(_vm.ClickId);
            }
        }
        private async Task ToggleSize()
        {
            if (!_vm.ReSizable)
                return;
            if (_vm.Size == "minimalized")
                _vm.Size = "large";
            else if (_vm.Size == "large")
            {
                _vm.Size = "minimalized";
                if (FooterClick.HasDelegate)
                    await FooterClick.InvokeAsync();
            }
        }
        public void Dispose()
        {
            OnClick = default!;
            FooterClick= default!;
            GC.SuppressFinalize(this);
        }
       
    }
}