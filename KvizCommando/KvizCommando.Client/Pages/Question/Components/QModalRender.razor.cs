using KvizCommando.Client.Features.Modal;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Pages.Question.Components
{
    public partial class QModalRender : KcComponentBase, IDisposable
    {
        //[Inject] private ILanguageService Lang { get; set; } = default!;
        [CascadingParameter] 
        public AppState Appstates { get; set; } = default!; 
        
        [CascadingParameter] 
        public int Mode { get; set; } = 0;

        [Parameter] public int  SlotNo { get; set; } = default!;
        //private MarkupString renderHTML { get; set; } = default;

        private bool isLoaded = false;
        private MarkupString renderHTML = new();
        private PendingSlot Slot => Appstates.Question!.PendingSlots[SlotNo] ?? new PendingSlot();
        private string culture => Appstates.Culture;
        private string? catName => CategoryNameLocalizer.GetCategory(Slot.Category, culture);
        private string? colorStyle => Slot.Status == "Approved" ? "color:lime" : "color:red";

        protected override async Task OnInitializedAsync()
        {
            if(!Appstates.LocStoreStates.ChkBxNotShowNew ?? false)
                renderHTML = await MarkupLoader.LoadingHtmlAsync(culture, Html.NewQuestRules);
            isLoaded = true;
        }
        public void Dispose() 
        { 
            GC.SuppressFinalize(this);
        }
    }
}
