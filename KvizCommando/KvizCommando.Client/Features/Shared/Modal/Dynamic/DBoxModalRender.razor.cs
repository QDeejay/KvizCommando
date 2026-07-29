using KvizCommando.Client.Features.Shared.Modal.Dynamic.Builders;
using KvizCommando.Client.Services.Visual.UiService.Language;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Shared.Modal.Dynamic
{
    public partial class DBoxModalRender
    {
        [Inject] private ILanguageService Lang { get; set; } = default!;

        [Parameter]
        public DBoxConfirmTypes ConfirmType { get; set; } =
            DBoxConfirmTypes.None;
    }
}

/**
 * ÚJ FÁJL: a dinamikus komponens egyetlen bemenete a ténylegesen deklarált
 * ConfirmType paraméter; ez vezérli a Razor switch ágát.
 */
