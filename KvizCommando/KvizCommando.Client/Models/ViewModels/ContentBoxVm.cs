using KvizCommando.Client.Features.Home;
using System.Security.Cryptography.X509Certificates;

namespace KvizCommando.Client.Models.ViewModels
{
    public sealed class ContentBoxVm
    {
        public string DictKey { get; init; } = "";
        public string Header { get; init; } = "";
        public string Footer { get; init; } = "";
        public bool FooterDisplay { get; init; }
        public string Size { get; set; } = "medium"; // small | medium | wide
        public bool ReSizable { get; init; } = false;
        public string ImageSrc { get; init; } = string.Empty;
        public string BgImageSrc { get; init; } = string.Empty;
        public int RenderContent { get; init; } = 0;
        public bool LcdDisplay { get; init; } = false;
        public bool IsClickable { get; init; } = true;
        public bool IsEnabled { get; init; } 
        public int ClickId { get; init; }
        public bool ButtonBarVisible { get; init; } = false;
        public Type? BodyComponent { get; set; }
    }
    public abstract class ButtonVm
    {   
      
        public string TitleKey { get; init; } = default!;
        public string ImageSrc { get; init; } = default!;
        public string BgImageSrc { get; init; } = default!;
        public int RenderContent { get; init; } = 0; 
        public bool LcdBackground { get; init; } = false;
        public string Size { get; init; } = "medium";
        public bool FooterDisplay { get; init; } = false;
        public bool ButtonBar { get; init; } = false;
        public int ClickId { get; init; } = 0;
        public Type? BodyComponent { get; set; }
    }
}
