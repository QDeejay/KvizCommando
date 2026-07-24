using KvizCommando.Client.Features.Question.Services;
using KvizCommando.Client.Pages.Shared.Modal.Dynamic;
using KvizCommando.Client.Pages.Shared.Modal.Features;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Question.Components;

public partial class UserSlotManager
{
    [Inject] private ILanguageService Lang { get; set; } = default!;
    [Inject] private IQuestionClientService QuestionService { get; set; } = default!;
    [Inject] private UiServices Ui { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    private int _selectedId = 100;

    private string Culture => AppStates.Culture;
    private UserSlot[] Slots => AppStates.Question!.Userlots;
    private QuestionExtendedInfo ExtInfo =>
        AppStates.Question!.ExtendedInfo;
    private bool NotShowStat =>
        AppStates.LocStoreStates.ChkBxNotShowDel ?? false;

    private void OnSelect(int id)
    {
        _selectedId = _selectedId == id ? 100 : id;
    }

    private async Task OnWatchButtonAsync()
    {
        if (_selectedId == 100)
            return;

        var modal = MBoxBuilder.BuildParam(
            ModalTypes.QCheckQuestion,
            Ui.Lang);
        modal.BodyParameters.Add(
            nameof(QModalRender.SlotNo),
            _selectedId);
        await Ui.Modal.ShowAsync(modal);
    }

    private async Task OnHandleButtonAsync()
    {
        if (_selectedId == 100)
            return;

        if (!NotShowStat)
        {
            var modal = MBoxBuilder.BuildParam(
                ModalTypes.QUsrDelet,
                Ui.Lang);

            if (await Ui.Modal.ShowAsync(modal) != ModalResult.Button1)
                return;
        }

        var success = await QuestionService.ManageSlotAsync(
            new ManageSlotRequest
            {
                SlotNo = _selectedId,
                ReqType = SlotManageType.DeleteUsr
            });

        if (!success)
            return;

        _selectedId = 100;
        await Ui.ReloadAsync(ReqStates.All);
    }
}
