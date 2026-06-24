using KvizCommando.Client.Services.Language;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KvizCommando.Client.Pages.Shared
{
    public partial class ScreenTooSmallLayer : ComponentBase
    {
        [Inject] protected ILanguageService Lang { get; set; } = default!;
    }
}
