using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Components
{
    public partial class PageContainer
    {
        [Parameter]
        public RenderFragment? ChildContent { get; set; }
    }
}