using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Components;

public partial class HotKeyHandler
{
    [Parameter] public bool IsFullScreenGame { get; set; }
    [Parameter] public EventCallback<string> OnKey { get; set; }

    private Task SendAsync(string key) => OnKey.InvokeAsync(key);
}
