using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Drawing;

namespace KvizCommando.Client.Components
{
    public partial class ContentBox : IDisposable
    {
        [Parameter] public ContentBoxVm Vm { get; set; } = default!;
        [Parameter] public EventCallback<int> OnClick { get; set; }
        [Parameter] public EventCallback FooterClick { get; set; }
        [Parameter] public RenderFragment? ChildContent1 { get; set; }
        [Parameter] public RenderFragment? ChildContent2 { get; set; }
        [Parameter] public RenderFragment? ChildContent3 { get; set; }
        [Parameter] public RenderFragment? ChildContent4 { get; set; }
        [Parameter] public RenderFragment? ChildContent5 { get; set; }
        private ContentBoxVm _vm = new ContentBoxVm();
        private bool _noContent = true;
        protected override void OnParametersSet()
        {
            _vm = Vm;
            _noContent = _vm.RenderContent==0 &&  string.IsNullOrEmpty(_vm.ImageSrc);
            Console.WriteLine("V");
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