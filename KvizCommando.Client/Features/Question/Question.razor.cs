using KvizCommando.Client.Features.Question.Builders;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Features.Home.Builders;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Rules;
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
        Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.Question"], (int)HomeBoxKey.Question);
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
            case (int)QBoxKeyRoot.Factory:
                _boxOrder = QBoxBuilder.SubFact;
                headerTitle = _boxes[
                    QBoxKeyRoot.Factory.ToString()].Header;
                break;

            case (int)QBoxKeyRoot.Usr:
                _boxOrder = QBoxBuilder.SubUsr;
                headerTitle = _boxes[
                    QBoxKeyRoot.Usr.ToString()].Header;
                break;

            case (int)QBoxKeyRoot.Pending:
                _boxOrder = QBoxBuilder.SubPend;
                headerTitle = _boxes[
                    QBoxKeyRoot.Pending.ToString()].Header;
                break;

            case (int)QBoxKeyRoot.New:
                _boxOrder = QBoxBuilder.SubNew;
                headerTitle = _boxes[
                    QBoxKeyRoot.New.ToString()].Header;
                break;
        }

        Ui.Header.SetTitle(headerTitle, boxId);
        Ui.Header.SetBackBtnEna(boxId > (int)HomeBoxKey.Question);
        StateHasChanged();
    }

    private void HandleBack()
    {
        if (Ui.Header.PageIndex == (int)HomeBoxKey.Question)
        {
            Ui.Nav.NavigateTo("/home");
            return;
        }

        OnBoxClick((int)HomeBoxKey.Question);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= HandleBack;
        GC.SuppressFinalize(this);
    }
}
