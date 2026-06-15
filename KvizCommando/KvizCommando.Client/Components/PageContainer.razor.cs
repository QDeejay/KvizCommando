using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Components
{
    public partial class PageContainer : ComponentBase
    {
        [Parameter]
        public RenderFragment? ChildContent { get; set; }
    }
}