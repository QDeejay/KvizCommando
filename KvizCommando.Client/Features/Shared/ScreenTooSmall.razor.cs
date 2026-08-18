using KvizCommando.Client.Services.Visual.UiService.Language;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Shared
{
    public partial class ScreenTooSmall : ComponentBase
    {
        [Inject] protected ILanguageService Lang { get; set; } = default!;
    }
}
