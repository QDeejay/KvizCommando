using KvizCommando.Client.Features.Question.Builders;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Question;

public partial class Question : KcComponentBase, IDisposable
{
    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    private readonly Dictionary<string, ContentBoxVm> _boxes = [];

    private string[] _boxOrder = [];
    private bool _hasAccess;
    private bool _isReady;
    //private bool _isSubscribed;

    private string Culture => AppStates.Culture;
    private QuestionDtos QuestionData => AppStates.Question!;

    protected override async Task OnInitializedAsync()
    {
        var teamLevel =
            AppStates.Home?.UserMainData.RankEnum ?? 0;

        if (teamLevel <= 0)
        {
            Ui.Nav.NavigateTo("/home", replace: true);
            return;
        }

        var expectedLoadoutSize =
            QuestionLoadoutRules.GetLoadoutSize(teamLevel);

        if (AppStates.Question is null ||
            AppStates.Question.AccessDenied ||
            AppStates.Question.FactorySlots.Length !=
                expectedLoadoutSize)
        {
            await Ui.ReloadAsync(ReqStates.Question);
        }

        if (AppStates.Question is null ||
            AppStates.Question.AccessDenied)
        {
            Ui.Nav.NavigateTo("/home", replace: true);
            return;
        }

        _hasAccess = true;
        Ui.Header.OnBackBtnClicked += HandleBack;
        Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.Question"], 1);
        _boxOrder = QBoxBuilder.Root;
    }
    protected override void OnParametersSet()
    {
        if (_isReady && _hasAccess && AppStates.Question is not null)
            BuildBoxes();
    }

    private ContentBoxVm Box(string key) => _boxes[key];

    private void BuildBoxes()
    {
        if (!_hasAccess || AppStates.Question is null)
            return;

        foreach (var box in QBoxBuilder.BuildBoxes(
                     QuestionData.ExtendedInfo!,
                     Ui.Lang))
        {
            _boxes[box.Key] = box.Value;
        }

        _isReady = true;
    }

    private void OnBoxClick(int boxId)
    {
        _boxOrder = QBoxBuilder.Root;
        var headerTitle = Ui.Lang["mainlayout.Header.Question"];

        switch (boxId)
        {
            case 101:
                _boxOrder = QBoxBuilder.SubFact;
                headerTitle = _boxes[
                    QBoxKeyRoot.RtBtnFactory.ToString()].Header;
                break;

            case 102:
                _boxOrder = QBoxBuilder.SubUsr;
                headerTitle = _boxes[
                    QBoxKeyRoot.RtBtnUsr.ToString()].Header;
                break;

            case 103:
                _boxOrder = QBoxBuilder.SubPend;
                headerTitle = _boxes[
                    QBoxKeyRoot.RtBtnPendig.ToString()].Header;
                break;

            case 104:
                _boxOrder = QBoxBuilder.SubNew;
                headerTitle = _boxes[
                    QBoxKeyRoot.RtBtnNew.ToString()].Header;
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

        //BuildBoxes();
        OnBoxClick(1);
    }

    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= HandleBack;
        GC.SuppressFinalize(this);
    }
}

/**
 * MÓDOSÍTÁS: 0-s csapatszinten a közvetlen /question navigáció is
 * visszairányít a Home oldalra. Szintváltás után a lap csak akkor kér
 * új Question snapshotot, ha a cache hiányzik, tiltott, vagy a benne
 * lévő loadout hossza már nem egyezik a 6/8/10-es szintszabállyal.
 */
