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
      
        protected bool _isLoaded = false;

        private QuestionExtendedInfo ExtInfo => AppStates.Question!.ExtendedInfo;
        private PendingSlot[] Slots => AppStates.Question!.PendingSlots;
        private string Culture => AppStates.Culture;

       

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
                await SelectedIdChanged.InvokeAsync(id);
            Console.WriteLine($"Selected:{SelectedId}");
        }
        public void Dispose()
        {
            
            SelectedIdChanged = default!;
            GC.SuppressFinalize(this);
        }
    }
}
/*
     <div class="lcd-display-outer">
        <div class="lcd-display-inner">
        </div>
    </div>
   
 *
         //[Parameter] public QuestionExtendedInfo ExtInfo { get; set; } = default!;
        //[Parameter] public PendingSlot[] Slots { get; set; //} = default!;
        //private int SelectedId { get; set; }
        // EventCallback<int> SelectedIdChanged { get; set; }
  protected bool btnHandleEna => SelectedId != 100 && Slots[SelectedId].Status != "Pending" && Slots[SelectedId].Category != 0 ? true : false;
        protected bool btnNewEna => SelectedId != 100 && Slots[SelectedId].Category == 0 ? true : false;


 * HandleSlot = default;
 *         protected async Task OnHandButton(int selectedId)
        {
            if (HandleSlot.HasDelegate)
                await HandleSlot.InvokeAsync(selectedId);
        }
 

 * NextScreen = default;
 * [Parameter] public EventCallback<int> NextScreen { get; set; }
 protected async Task OnNextScreen(int screenId)
        {
            if (NextScreen.HasDelegate)
                await NextScreen.InvokeAsync(screenId);
        }
 <button class="military-button--secondary"
                @onclick="@(() => OnNextScreen(1))">
            @Lang["question.Button.ToUsr"]
        </button>
        <button class="military-button"
                disabled="@(!btnNewEna)"
                @onclick="@(() => OnNextScreen(3 + SelectedId))">
            @Lang["question.Button.New"]
        </button>
 
  <div class="usr-bar"> 
        
        
        <button class="military-button"
                disabled="@(!btnHandleEna)"
                @onclick="@(() => OnHandButton(SelectedId))">
            @Lang["question.Button.Handle"]
        </button>
       
    </div>
 */