using KvizCommando.Client.Services.Visual.UiService.Language;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KvizCommando.Client.Features.Shared
{
    public partial class ScreenTooSmallLayer : ComponentBase
    {
        [Inject] protected ILanguageService Lang { get; set; } = default!;
    }
}
