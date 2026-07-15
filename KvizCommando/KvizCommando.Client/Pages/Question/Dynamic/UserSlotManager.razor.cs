
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;


namespace KvizCommando.Client.Pages.Question.Dynamic
{
    public partial class UserSlotManager : IDisposable
    {
        [Inject] ILanguageService Lang { get; set; } = default!;
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;

        [CascadingParameter]
        private int SelectedId { get; set; }
        [Parameter] public Action<int>? SelectedIdChanged { get; set; }
        [Parameter] public Func<Task>? OnHandleButtonPushed { get; set; }
        [Parameter] public Func<Task>? OnWatchButtonPushed { get; set; }

        private bool _isLoaded = false;

        private string Culture => AppStates.Culture;
        private UserSlot[] Slots => AppStates.Question!.Userlots;
        private QuestionExtendedInfo ExtInfo => AppStates.Question!.ExtendedInfo;
        private bool NotShowStat => AppStates.LocStoreStates.ChkBxNotShowDel ?? false;

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

        private async Task OnWatchButtonAsync()
        {
            if (OnWatchButtonPushed is not null)
                await OnWatchButtonPushed.Invoke();
        }
        private async Task OnHandleButtonAsync()
        {
            if (OnHandleButtonPushed is not null)
                await OnHandleButtonPushed.Invoke();
        }

        public void Dispose()
        {
            SelectedIdChanged = default;
            OnWatchButtonPushed = default;
            OnHandleButtonPushed = default;
            GC.SuppressFinalize(this);
        }
    }
}



//[Parameter] public EventCallback<int> SelectedIdChanged { get; set; } = default!;
//[Parameter] public EventCallback OnWatchButtonPushed { get; set; } = default!;
//[Parameter] public EventCallback OnHandleButtonPushed { get; set; } = default!;


//if (OnHandleButtonPushed.HasDelegate)
//  await OnHandleButtonPushed.InvokeAsync();
//if (SelectedIdChanged.HasDelegate)
//  await SelectedIdChanged.InvokeAsync(SelectedId);
//if (OnWatchButtonPushed.HasDelegate)
//  await OnWatchButtonPushed.InvokeAsync();