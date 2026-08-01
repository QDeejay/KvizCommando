using KvizCommando.Client.Features.Question.Services;
using KvizCommando.Client.Features.Shared.Modal.Builders;
using KvizCommando.Client.Features.Shared.Modal.Dynamic;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.Question;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace KvizCommando.Client.Features.Question.Components;

public partial class NewQuestionManager
{
    [Inject] private ILanguageService Lang { get; set; } = default!;
    [Inject] private CategoryOptionHelpers CatHelper { get; set; } = default!;
    [Inject] private IQuestionClientService QuestionService { get; set; } = default!;
    [Inject] private UiServices Ui { get; set; } = default!;
    [Inject] private MarkupLoaderService MarkupLoader { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    private const int LENGHT_AREA_BOX = 200;
    private const int LENGHT_ANSWER_BOX = 40;

    private NewQuestionRequest _formData = new();

    private string Culture => AppStates.Culture;
    private bool LocNotShowStateNew =>
        AppStates.LocStoreStates.ChkBxNotShowNew ?? false;
    private bool[] CharCatMask =>
        AppStates.Question!.ExtendedInfo.CharCatMask;
    private int SelectedId => Array.FindIndex(
        AppStates.Question!.PendingSlots,
        slot => slot.Category == 0);
    private bool NoFreeSlot => SelectedId < 0;
    private bool DisabledLcd => _formData.Category == 0 || NoFreeSlot;
    private bool DisabledAnswer =>
        _formData.Question.Length < 10 ||
        _formData.Category == 0 ||
        !_formData.Question.Contains('?') ||
        NoFreeSlot;
    private bool DisabledSendButton =>
        DisabledLcd ||
        DisabledAnswer ||
        _formData.Answers.Any(string.IsNullOrWhiteSpace) ||
        _formData.Answers.Distinct().Count() != _formData.Answers.Length;
    private string DisCursor => DisabledLcd
        ? "cursor: url('/Images/cursors/disabled.cur'), not-allowed !Important;"
        : string.Empty;
    private string DisBckGround => DisabledLcd
        ? "background-color: #2a2a2a"
        : string.Empty;
    private CategoryOption[] Options => CatHelper.OptionsUpdate(
        CategoryOptionHelpers.optionType.New,
        CharCatMask);

    private async Task OnSaveQuestionAsync()
    {
        if (NoFreeSlot)
            return;

        if (!LocNotShowStateNew)
        {
            var htmlContent = await MarkupLoader.LoadingHtmlAsync(
                Culture,
                Html.NewQuestRules);
            var modal = MBoxBuilder.BuildParam(
                ModalTypes.QNewRules,
                Ui.Lang);
            modal.BodyParameters.Add(
                nameof(QModalRender.RenderHTML),
                htmlContent);

            if (await Ui.Modal.ShowAsync(modal) != ModalResult.Button1)
                return;
        }

        _formData.SlotNo = SelectedId;

        if (!await QuestionService.SendNewQuestionAsync(_formData))
            return;

        ReqStates[] refreshTypes = LocNotShowStateNew
            ? [ReqStates.Question]
            : [ReqStates.Question, ReqStates.LocalSotrage];


        await Ui.ReloadAsync(refreshTypes);

        _formData = new NewQuestionRequest();
    }

    private void OnEditorKeyDown(KeyboardEventArgs e)
    {
        if (e.Key is "Enter" or "Escape")
            StateHasChanged();
    }
}
