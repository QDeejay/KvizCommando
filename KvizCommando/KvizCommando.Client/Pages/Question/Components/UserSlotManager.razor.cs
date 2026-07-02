
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;


namespace KvizCommando.Client.Pages.Question.Components
{
    public partial class UserSlotManager : KcComponentBase, IDisposable
    {
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;
        [Parameter] public int SelectedId { get; set; }
        [Parameter] public EventCallback<int> SelectedIdChanged { get; set; } = default!;

        //private int SelectedId = 100;
        private bool _isLoaded = false;

        private string Culture => AppStates.Culture;
        private UserSlot[] Slots => AppStates.Question!.Userlots;
        private QuestionExtendedInfo ExtInfo => AppStates.Question!.ExtendedInfo;

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
