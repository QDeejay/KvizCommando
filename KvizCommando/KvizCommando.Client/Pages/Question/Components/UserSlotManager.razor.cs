
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;


namespace KvizCommando.Client.Pages.Question.Components
{
    public partial class UserSlotManager : KcComponentBase, IDisposable
    {
        //[Inject] private  ILanguageService Lang { get; set; } = default!;
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;
        [Parameter] public int SelectedId { get; set; }
        [Parameter] public EventCallback<int> SelectedIdChanged { get; set; } = default!;

        //private int SelectedId = 100;
        private bool _isLoaded = false;

        private string Culture => AppStates.Culture;
        private UserSlot[] Slots => AppStates.Question!.Userlots;
        private QuestionExtendedInfo ExtInfo => AppStates.Question!.ExtendedInfo;
        //private bool NotShowDelete => AppStates.LocStoreStates.ChkBxNotShowDel ?? false;

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
                //qModal = qModal with { Mode = 0 };
            }
            else SelectedId = id;
            if (SelectedIdChanged.HasDelegate)
                await SelectedIdChanged.InvokeAsync(SelectedId);
            Console.WriteLine($"Selected:{SelectedId}");
            
        }
        public void Dispose()
        {
            SelectedId = default!;
            SelectedIdChanged = default!;
            GC.SuppressFinalize(this);
        }
    }
}
/*     
 *             //[Parameter] public QuestionExtendedInfo ExtInfo { get; set; } = default!;
        //[Parameter] public bool NotShowDel { get; set; } = default;
        //[Parameter] public UserSlot[] Slots { get; set; } = default!;
 *     
 *     protected async Task OnUsrButton()
        {
            if (HandleSlot.HasDelegate)
                await HandleSlot.InvokeAsync(SelectedId);
        }
 *         protected string UsrButtonString => NotShowDel ? Lang["question.Button.Delete"] : Lang["question.Button.Handle"];
        protected string UsrButtonStyle => NotShowDel ? "background-color: #a64b2a" : "background-color: #4b5320";
        protected Func<Task> UsrButtonAction => NotShowDel ? () => OnDelButton() : () => OnUsrButton();
        protected string gotoInfo => ExtInfo.HandlePendingSlot > 0 ? $"({ExtInfo.HandlePendingSlot})" : "";
 * 
 *  [Parameter] public EventCallback<int> HandleSlot { get; set; }
        [Parameter] public EventCallback<int> DeleteSlot { get; set; }
 *   protected async Task OnNextScreen()
        {
            if (NextScreen.HasDelegate)
                await NextScreen.InvokeAsync(2);
        }
 <button class="military-button--secondary"
                @onclick="@OnNextScreen">
            @Lang["question.Button.ToPending"] @gotoInfo
        </button>
         [Parameter] public EventCallback<int> NextScreen { get; set; }
  protected async Task OnDelButton()
        {
            if (DeleteSlot.HasDelegate)
                await DeleteSlot.InvokeAsync(SelectedId);
        }
    <div class="lcd-display-outer">
        <div class="lcd-display-inner">
        </div>
    </div>
    
 */