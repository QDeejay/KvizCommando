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
        public string DisableTextTp { get; init; } = string.Empty;
        public int RenderContent { get; init; } = 0;
        public bool LcdDisplay { get; init; } = false;
        public bool IsClickable { get; init; } = true;
        public bool IsEnabled { get; init; }
        public int ClickId { get; init; }
        public Type? BodyComponent { get; set; }
        public Dictionary<string, object?> BodyParameters { get; init; } = new();
    }
    public abstract class VmSpecs
    {

        public string TitleKey { get; init; } = default!;
        public string ImageSrc { get; init; } = default!;
        public string BgImageSrc { get; init; } = default!;
        public int RenderContent { get; init; } = 0;
        public bool LcdBackground { get; init; } = false;
        public string Size { get; init; } = "medium";
        public bool FooterDisplay { get; init; } = false;
        public int ClickId { get; init; } = 0;
        public Type? BodyComp { get; init; }
    }

    public sealed class ContentBoxSize
    {
        public const string BUTTON_SMALL = "small";
        public const string BUTTON_MEDIUM = "medium";
        public const string BUTTON_WIDE = "wide";
        public const string BUTTON_TALL = "tall";
        public const string MINIMALIZED = "minimalized";
        public const string CONTENT_LARGE = "large";
        public const string CONTENT_EXTRA_LARGE = "extra-large";
        public const string CONTENT_CLOSED_LARGE = "closed-large";
        public const string CONTENT_HALF_LARGE = "halflarge";
        public const string CONTENT_FLEXIBLE = "content-sized";

    }

}
