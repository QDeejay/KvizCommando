using KvizCommando.Client.Features.Question.Builders;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Question;

public partial class Question : KcComponentBase, IDisposable
{
    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    private string[] _boxOrder = [];
    private bool _isReady;

    private string Culture => AppStates.Culture;
    private QuestionDtos QuestionData => AppStates.Question!;

    protected override void OnInitialized()
    {
        Ui.Header.OnBackBtnClicked += HandleBack;
        Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.Question"], 1);
        _boxOrder = QBoxBuilder.Root;
    }

    private Dictionary<string, ContentBoxVm> BuildBoxes() =>
        QBoxBuilder.BuildBoxes(
            QuestionData.ExtendedInfo!,
            Ui.Lang);

    private void SetReady() => _isReady = true;

    private void OnBoxClick(int boxId)
    {
        var boxes = BuildBoxes();
        _boxOrder = QBoxBuilder.Root;
        var headerTitle = Ui.Lang["mainlayout.Header.Question"];

        switch (boxId)
        {
            case 101:
                _boxOrder = QBoxBuilder.SubFact;
                headerTitle = boxes[QBoxKeyRoot.RtBtnFactory.ToString()].Header;
                break;

            case 102:
                _boxOrder = QBoxBuilder.SubUsr;
                headerTitle = boxes[QBoxKeyRoot.RtBtnUsr.ToString()].Header;
                break;

            case 103:
                _boxOrder = QBoxBuilder.SubPend;
                headerTitle = boxes[QBoxKeyRoot.RtBtnPendig.ToString()].Header;
                break;

            case 104:
                _boxOrder = QBoxBuilder.SubNew;
                headerTitle = boxes[QBoxKeyRoot.RtBtnNew.ToString()].Header;
                break;
        }

        Ui.Header.SetTitle(headerTitle, boxId);
        Ui.Header.SetBackBtnEna(boxId > 1);
        StateHasChanged();
    }

    private void HandleBack()
    {
        if (Ui.Header.PageIndex == 1)
        {
            Ui.Nav.NavigateTo("/home");
            return;
        }

        OnBoxClick(1);
    }

    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= HandleBack;
        GC.SuppressFinalize(this);
    }
}
