using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Shared
{
    public partial class ScreenTooSmall : ComponentBase
    {
        [Parameter] public string WarningText { get; set; } = string.Empty;
    }
}
