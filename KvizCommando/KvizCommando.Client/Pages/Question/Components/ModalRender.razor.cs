using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Language;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Pages.Question.Components
{
    public partial class ModalRender : ComponentBase
    {
        [Inject] protected ILanguageService Lang { get; set; } = default!;

        [Parameter] public int Mode { get; set; } = default!;
        [Parameter] public PendingSlot Slot { get; set; } = default!;
        [Parameter] public MarkupString renderHTML { get; set; } = default;

        protected bool isLoaded = false;
        protected string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        protected string? catName => CategoryNameLocalizer.GetCategory(Slot.Category, culture);
        protected string? colorStyle => Slot.Status == "Approved" ? "color:lime" : "color:red";

        protected override void OnInitialized()
        {
            isLoaded = true;
        }
    }
}
