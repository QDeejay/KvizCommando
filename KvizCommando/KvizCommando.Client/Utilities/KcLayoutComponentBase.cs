using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual.UiService;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Utilities
{
    public abstract class KcLayoutComponentBase : LayoutComponentBase
    {
        [Inject] protected UiServices Ui { get; set; } = default!;
        //[Inject] protected ModalService Modal { get; set; } = default!;
        //[Inject] protected NavigationManager Nav { get; set; } = default!;
        //[Inject] protected ILanguageService Lang { get; set; } = default!;
        [Inject] protected IApiService Api { get; set; } = default!;
        [Inject] protected IUserService User { get; set; } = default!;

        //[Inject] protected ToastService Toast { get; set; } = default!;
        
    }
}
