using KvizCommando.Client.Features.Question.Services;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Rules;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace KvizCommando.Client.Features.Question.Components;

public partial class FactorySlotsBase
{
    [Inject] private ILanguageService Lang { get; set; } = default!;
    [Inject] private CategoryOptionHelpers CatHelper { get; set; } = default!;
    [Inject] private IQuestionClientService QuestionService { get; set; } = default!;
    [Inject] private UiServices Ui { get; set; } = default!;
    [Inject] private AudioService Audio { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    private int? _editingRowIndex;
    private CategoryOption[] _options = [];
    private int[] _originalCodes = [];
    private int[] _workingCodes = [];

    private string Culture => AppStates.Culture;
    private bool IsDirty =>
        !QuestionHelper.ArraysEqual(_originalCodes, _workingCodes);
    private int[] FactSlots => AppStates.Question!.FactorySlots;
    private QuestionExtendedInfo ExtInfo =>
        AppStates.Question!.ExtendedInfo;
    private int OwnQuestionLimit =>
        QuestionLoadoutRules.GetOwnQuestionLimit(
            _workingCodes.Length,
            ExtInfo.OccupiedUserSlot);

    protected override void OnInitialized()
    {
        (_originalCodes, _workingCodes) =
            QuestionHelper.CloneFactorySlots(FactSlots);
    }

    private async Task StartEdit(int rowIndex)
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _editingRowIndex = rowIndex;
        StateHasChanged();
    }

    private void StopEdit()
    {
        _editingRowIndex = null;
        StateHasChanged();
    }

    private async Task OnSaveSlotsAsync()
    {
        if (!IsDirty)
            return;

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        StopEdit();

        var success = await QuestionService.SaveFactorySlotsAsync(
            new SaveFactoryRequest { CategorySlots = _workingCodes });

        if (!success)
            return;

        await Ui.ReloadAsync(ReqStates.Question);
        (_originalCodes, _workingCodes) =
            QuestionHelper.CloneFactorySlots(FactSlots);
    }

    private void OnEditorKeyDown(KeyboardEventArgs e)
    {
        if (e.Key is "Enter" or "Escape")
            StopEdit();
    }

    private bool CanSelectOwnQuestion(int rowIndex)
    {
        return rowIndex < OwnQuestionLimit;
    }
}
