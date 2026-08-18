using KvizCommando.Client.Features.Shared.Modal.Builders;
using KvizCommando.Client.Services.Visual.UiService.Language;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Shared.Modal.Components
{
    public partial class DBoxModalRender
    {
        [Inject] private ILanguageService Lang { get; set; } = default!;

        [Parameter]
        public DBoxConfirmTypes DialogBoxType { get; set; } =
            DBoxConfirmTypes.None;

        [Parameter]
        public string RequestedLanguage { get; set; } = string.Empty;
    }
}
