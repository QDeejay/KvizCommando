using KvizCommando.Client.Features.Home;

namespace KvizCommando.Client.Models.ViewModels
{
    public sealed class ContentBoxVm
    {
        public string Header { get; init; } = "";
        public string Footer { get; init; } = "";
        public bool FooterDisplay { get; init; }
        public string Size { get; init; } = "medium"; // small | medium | wide
        public string ImageSrc { get; init; } = string.Empty;
        public bool IsClickable { get; init; } = true;
        public bool IsEnabled { get; init; }
        public int ClickId { get; init; }
    }
    public abstract class ButtonVm
    {   
        public string TitleKey { get; init; } = default!;
        public string ImageSrc { get; init; } = default!;
        public string Size { get; init; } = "medium";
        public bool FooterDisplay { get; init; }
        public int ClickId { get; init; }

    }
}
