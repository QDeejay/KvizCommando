using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;


namespace KvizCommando.Client.Pages.Question.Components
{
    public partial class PendingSlotManager : KcComponentBase, IDisposable
    {
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;
        [Parameter] public int SelectedId { get; set; } = default!;
        [Parameter] public EventCallback<int> SelectedIdChanged { get; set; }
      
        private bool _isLoaded = false;

        private string Culture => AppStates.Culture;
        private QuestionExtendedInfo ExtInfo => AppStates.Question!.ExtendedInfo;
        private PendingSlot[] Slots => AppStates.Question!.PendingSlots;
      
        protected override async Task OnInitializedAsync()
        {
            await OnSelect(100);
            _isLoaded = true;
        }
        
        private async Task OnSelect(int id)
        {
            if (SelectedId == id)
            {
                SelectedId = 100;
            }
            else SelectedId = id;
            if (SelectedIdChanged.HasDelegate)
                await SelectedIdChanged.InvokeAsync(SelectedId);
            Console.WriteLine($"Selected:{SelectedId}");
        }
        public void Dispose()
        {
            SelectedIdChanged = default;
            GC.SuppressFinalize(this);
        }
    }
}
