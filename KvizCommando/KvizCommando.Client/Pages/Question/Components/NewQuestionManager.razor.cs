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
        [CascadingParameter]
        private int SelectedId { get; set; }

        [Inject] private CategoryOptionHelpers CatHelper { get; set; } = default!;
        [Parameter] public Func<NewQuestionRequest, Task>? OnSendQuestion { get; set; }

        private const int LENGHT_AREA_BOX = 200;
        private const int LENGHT_ANSWER_BOX = 40;

        private readonly NewQuestionRequest _formData = new();

        private bool _isLoaded = false;
       
        private string Culture => AppStates.Culture;
        private bool[] CharCatMask => AppStates.Question!.ExtendedInfo.CharCatMask;
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
        private async Task OnSaveQuestionAsync()
        {
            _formData.SlotNo = SelectedId;
            if (OnSendQuestion is not null)
                await OnSendQuestion.Invoke(_formData);
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
//[Parameter] public EventCallback<NewQuestionRequest> OnSendQuestion { get; set; }

//if (OnSendQuestion.HasDelegate)
//    await OnSendQuestion.InvokeAsync(_formData);


