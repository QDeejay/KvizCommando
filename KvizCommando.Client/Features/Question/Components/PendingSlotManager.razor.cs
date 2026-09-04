using KvizCommando.Client.Features.Question.Services;
using KvizCommando.Client.Features.Shared.Modal.Builders;
using KvizCommando.Client.Features.Shared.Modal.Components;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Question.Components;

public partial class PendingSlotManager
{
    [Inject] private ILanguageService Lang { get; set; } = default!;
    [Inject] private IQuestionClientService QuestionService { get; set; } = default!;
    [Inject] private UiServices Ui { get; set; } = default!;
    [Inject] private AudioService Audio { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    private int _selectedId = 100;

    private string Culture => AppStates.Culture;
    private QuestionExtendedInfo ExtInfo =>
        AppStates.Question!.ExtendedInfo;
    private PendingSlot[] Slots => AppStates.Question!.PendingSlots;

    private void OnSelect(int id)
    {
        _selectedId = _selectedId == id ? 100 : id;
    }

    private async Task OnHandleButtonAsync()
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);

        if (_selectedId == 100)
            return;

        var modal = MBoxBuilder.BuildParam(
            ModalTypes.QPendHandle,
            Ui.Lang);
        modal = modal with
        {
            ActionText2 = Slots[_selectedId].Status == "Approved" &&
                ExtInfo.FreeUserSlot > 0
                ? modal.ActionText2
                : string.Empty
        };
        modal.BodyParameters.Add(
            nameof(QModalRender.SlotNo),
            _selectedId);

        var result = await Ui.Modal.ShowAsync(modal);
        var requestType = result switch
        {
            ModalResult.Button1 => SlotManageType.DeletePending,
            ModalResult.Button2 => SlotManageType.MovePending,
            _ => (SlotManageType?)null
        };

        if (requestType is null)
            return;

        var success = await QuestionService.ManageSlotAsync(
            new ManageSlotRequest
            {
                SlotNo = _selectedId,
                ReqType = requestType.Value
            });

        if (!success)
            return;

        _selectedId = 100;
        await Ui.ReloadAsync(ReqStates.Question);
    }
}
