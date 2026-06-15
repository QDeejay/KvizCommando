using KvizCommando.Client.Services.Language;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;


namespace KvizCommando.Client.Pages.Question.Components
{
    public partial class PendingSlotManager : ComponentBase
    {
        [Inject] protected ILanguageService Lang { get; set; } = default!;

        [Parameter] public QuestionExtendedInfo ExtInfo { get; set; } = default!;
        [Parameter] public PendingSlot[] Slots { get; set; } = default!;
        [Parameter] public EventCallback<int> HandleSlot { get; set; }
        

        protected string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        protected bool _isLoaded = false;
        private int SelectedId { get; set; }
        private EventCallback<int> SelectedIdChanged { get; set; }

        protected bool btnHandleEna => SelectedId != 100 && Slots[SelectedId].Status != "Pending" && Slots[SelectedId].Category != 0 ? true : false;
        protected bool btnNewEna => SelectedId != 100 && Slots[SelectedId].Category == 0 ? true : false;

        protected override async Task OnInitializedAsync()
        {
            SelectedId = 100;
            _isLoaded = true;
            await Task.Delay(1);
        }
        
        protected async Task OnHandButton(int selectedId)
        {
            if (HandleSlot.HasDelegate)
                await HandleSlot.InvokeAsync(selectedId);
        }
        private async Task OnSelect(int id)
        {
            if (SelectedId == id)
            {
                SelectedId = 100;
               
            }
            else SelectedId = id;
            await SelectedIdChanged.InvokeAsync(id);
            Console.WriteLine($"Selected:{SelectedId}");
        }
        protected void Dispose()
        {
            HandleSlot = default;
            SelectedIdChanged = default;
            GC.SuppressFinalize(this);
        }
    }
}
/*
 * 
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