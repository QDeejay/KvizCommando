using KvizCommando.Client.Helpers;
using KvizCommando.Client.Pages.Shared.Modal.Features;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Pages.Shared.Modal.Dynamic
{
    public partial class QModalRender : IDisposable
    {
        [Inject] private ILanguageService Lang { get; set; } = default!;

        [CascadingParameter]
        public AppState Appstates { get; set; } = default!;

        [CascadingParameter]
        public ModalTypes Mode { get; set; } = ModalTypes.None;

        [Parameter] public int SlotNo { get; set; } = default!;
        [Parameter] public MarkupString RenderHTML { get; set; } = default!;

        private bool isLoaded = false;
        //private MarkupString renderHTML = new();
        private PendingSlot Slot => Appstates.Question!.PendingSlots[SlotNo] ?? new PendingSlot();
        private UserSlot UsrSlot => Appstates.Question!.Userlots[SlotNo] ?? new UserSlot();
        private string Culture => Appstates.Culture;
        private string? CatNamePend => CategoryNameLocalizer.GetCategory(Slot.Category, Culture);
        private string? CatNameUsr => CategoryNameLocalizer.GetCategory(UsrSlot.Category, Culture);
        private string? ColorStyle => Slot.Status == "Approved" ? "color:lime" : "color:red";

        protected override async Task OnInitializedAsync()
        {
            isLoaded = true;
            await Task.Delay(1);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
