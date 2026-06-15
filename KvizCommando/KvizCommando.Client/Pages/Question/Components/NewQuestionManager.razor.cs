using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;

namespace KvizCommando.Client.Pages.Question.Components
{
    public partial class NewQuestionManager : ComponentBase
    {
        [Inject] protected ILanguageService Lang { get; set; } = default!;
        [Inject] protected CategoryOptionHelpers CatHelper { get; set; } = default!;

        [Parameter] public string MessageUsr { get; set; } = string.Empty;
        [Parameter] public QuestionExtendedInfo ExtInfo { get; set; } = default!;
        [Parameter] public bool isSuccess { get; set; } = false;
        [Parameter] public EventCallback<NewQuestionRequest> SendQuestion { get; set; }
        

        private NewQuestionRequest formData = new();
        protected string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        private bool _isLoaded = false;

        protected const int areaLenght = 200;
        protected const int answerLenght = 40;
        private bool disabledLcd => formData.Category == 0 || isSuccess;
        private bool disabledAnswer => formData.Question.Length < 10 || formData.Category == 0 || !formData.Question.Contains('?') || isSuccess;

        private bool disabledSendButton => disabledLcd || disabledAnswer || formData.Answers.Any(a => string.IsNullOrWhiteSpace(a)) || formData.Answers.Distinct().Count() != formData.Answers.Length || isSuccess;

        private string disCursor => disabledLcd ? "cursor: url('/Images/cursors/disabled.cur'), not-allowed !Important;" : "";
        private string disBckGround => disabledLcd ? "background-color: #2a2a2a" : "";

        private CategoryOption[] Options => CatHelper.OptionsUpdate(CategoryOptionHelpers.optionType.New, ExtInfo.CharCatMask);
        protected override void OnInitialized()
        {
            _isLoaded = true;
        }
        protected async Task OnSaveQuestion()
        {

            if (SendQuestion.HasDelegate)
                await SendQuestion.InvokeAsync(formData);
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
        protected void Dispose()
        {
            SendQuestion = default;
            GC.SuppressFinalize(this);
        }
    }
}

/*
  
  protected async Task OnNextScreen()
        {
            if (NextScreen.HasDelegate)
                await NextScreen.InvokeAsync(2);
        }
 NextScreen = default;
 [Parameter] public EventCallback<int> NextScreen { get; set; }
 */
