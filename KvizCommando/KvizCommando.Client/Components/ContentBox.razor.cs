using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace KvizCommando.Client.Components
{
    public partial class ContentBox : ComponentBase
    {
        [Parameter] public string Header { get; set; } = "";
        [Parameter] public string Footer { get; set; } = "";
        [Parameter] public string Size { get; set; } = "medium"; // small, medium, large, wide
        [Parameter] public bool IsEnabled { get; set; } = true;
        [Parameter] public bool IsClickable { get; set; } = false;
        [Parameter] public bool ShowImage { get; set; } = false;
        [Parameter] public string? ImageSrc { get; set; }
        [Parameter] public string? BackgroundImage { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public EventCallback<int> OnClick { get; set; }

        [Parameter] public EventCallback FooterClick { get; set; }
        [Parameter] public bool Resizable { get; set; } = false;
        [Parameter] public bool FooterDisplay { get; set; } = false;
        [Parameter] public int ClickId { get; set; }
        [Parameter] public bool LcdDisplay { get; set; } = false;
        private string CursorStyle { get; set; } = string.Empty;
        protected string CssClass =>
            $"content-box {Size}" + (IsClickable ? " clickable" : "");

        protected string BackgroundStyle =>
            string.IsNullOrWhiteSpace(BackgroundImage)
                ? ""
                : $"background-image: url('{BackgroundImage}'); background-size: cover; background-position: center;";

        protected string FooterClass =>
            FooterDisplay ? "box-footerActive" : "box-footer label-text";
        protected async Task HandleClick(MouseEventArgs _)
        {
            if (IsClickable && IsEnabled)
            {
                await OnClick.InvokeAsync(ClickId);
            }
        }
        private async Task ToggleSize()
        {
            if (!Resizable)
                return;
            if (Size == "minimalized")
                Size = "large";
            else if (Size == "large")
            {
                Size = "minimalized";
                await FooterClick.InvokeAsync();
            }
        }
               
       
    }
}