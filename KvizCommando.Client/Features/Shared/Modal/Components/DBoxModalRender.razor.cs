using KvizCommando.Client.Features.Shared.Modal.Builders;
using KvizCommando.Localization.Shared.Modal.Dialog;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace KvizCommando.Client.Features.Shared.Modal.Components
{
    public partial class DBoxModalRender
    {
        [Inject] private IStringLocalizer<DialogModalResource> Lang { get; set; } = default!;

        [Parameter]
        public DBoxConfirmTypes DialogBoxType { get; set; } =
            DBoxConfirmTypes.None;

        [Parameter]
        public string RequestedLanguage { get; set; } = string.Empty;

    }
}
