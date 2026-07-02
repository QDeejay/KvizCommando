using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;

namespace KvizCommando.Client.Pages.Question.Components
{
    public partial class NewQuestionManager : KcComponentBase, IDisposable
    {
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;
    
        [Inject] private CategoryOptionHelpers CatHelper { get; set; } = default!;
       // [Parameter] public QuestionExtendedInfo ExtInfo { get; set; } = default!;
        [Parameter] public int SelectedId { get; set; } 
        [Parameter] public EventCallback<NewQuestionRequest> OnSendQuestion { get; set; }

        private const int LENGHT_AREA_BOX = 200;
        private const int LENGHT_ANSWER_BOX = 40;

        private bool _isLoaded = false;

        private NewQuestionRequest _formData=new();

        private bool[] CharCatMask => AppStates.Question!.ExtendedInfo.CharCatMask;
        private string Culture => AppStates.Culture;
        private bool DisabledLcd => _formData.Category == 0 || SelectedId==100;
        private bool DisabledAnswer => _formData.Question.Length < 10 || _formData.Category == 0 || !_formData.Question.Contains('?') || SelectedId == 100;
        private bool DisabledSendButton => DisabledLcd || DisabledAnswer || _formData.Answers.Any(a => string.IsNullOrWhiteSpace(a)) || _formData.Answers.Distinct().Count() != _formData.Answers.Length || SelectedId == 100;
        private string DisCursor => DisabledLcd ? "cursor: url('/Images/cursors/disabled.cur'), not-allowed !Important;" : "";
        private string DisBckGround => DisabledLcd ? "background-color: #2a2a2a" : "";
        private CategoryOption[] Options => CatHelper.OptionsUpdate(CategoryOptionHelpers.optionType.New, CharCatMask);

        protected override void OnInitialized()
        {
          
            _isLoaded = true;
        }
        protected async Task OnSaveQuestion()
        {
            _formData.SlotNo = SelectedId;
            if (OnSendQuestion.HasDelegate)
                await OnSendQuestion.InvokeAsync(_formData);
        }
       
        private void StopEdit()
        {
            //EditingRowIndex = null;
            StateHasChanged();
        }

        private void OnEditorKeyDown(KeyboardEventArgs e)
        {
            // Egyszerű és megbízható kilépés: Enter vagy Esc
            if (e.Key == "Enter" || e.Key == "Escape")
            {
                StopEdit();
            }
        }
        public void Dispose()
        {
            OnSendQuestion = default;
            GC.SuppressFinalize(this);
        }
    }
}

/*
 * 
 * <p class="message-row">@MessageUsr</p>
          // [Inject] private ILanguageService Lang { get; set; } = default!;
        // [Parameter] public string MessageUsr { get; set; } = string.Empty;
  protected async Task OnNextScreen()
        {
            if (NextScreen.HasDelegate)
                await NextScreen.InvokeAsync(2);
        }
 NextScreen = default;
 [Parameter] public EventCallback<int> NextScreen { get; set; }
 */
