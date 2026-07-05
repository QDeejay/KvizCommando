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

        [CascadingParameter]
        private int SelectedId { get; set; }

        [Parameter] public Action<int>? SelectedIdChanged { get; set; }
        [Parameter] public Func<Task>? OnHandleButtonPushed { get; set; }

        private bool _isLoaded = false;

        private string Culture => AppStates.Culture;
        private QuestionExtendedInfo ExtInfo => AppStates.Question!.ExtendedInfo;
        private PendingSlot[] Slots => AppStates.Question!.PendingSlots;
      
        protected override async Task OnInitializedAsync()
        {
            OnSelect(100);
            await Task.Delay(1);
            _isLoaded = true;
        }

        private void OnSelect(int id)
        {
            if (SelectedId == id)
            {
                SelectedId = 100;
            }
            else SelectedId = id;
            if (SelectedIdChanged is not null)
                SelectedIdChanged?.Invoke(SelectedId);
        }
        private async Task OnHandleButtonAsync()
        {
            if (OnHandleButtonPushed is not null)
                await OnHandleButtonPushed.Invoke();
        }
        public void Dispose()
        {
            OnHandleButtonPushed = default;
            SelectedIdChanged = default;
            GC.SuppressFinalize(this);
        }
    }
}
// [Parameter] public EventCallback<int> SelectedIdChanged { get; set; }
//[Parameter] public EventCallback OnHandleButtonPushed { get; set; } = default!;

//if (SelectedIdChanged.HasDelegate)
//  await SelectedIdChanged.InvokeAsync(SelectedId);
//if (OnHandleButtonPushed.HasDelegate)
//  await OnHandleButtonPushed.InvokeAsync();
