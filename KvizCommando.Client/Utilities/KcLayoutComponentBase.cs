using KvizCommando.Client.Services.ScreenData;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual.UiService;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Utilities
{
    public abstract class KcLayoutComponentBase : LayoutComponentBase
    {
        [Inject] protected UiServices Ui { get; set; } = default!;
        [Inject] protected IUserService User { get; set; } = default!;

        
    }
}
