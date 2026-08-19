using KvizCommando.Client.Features.Shared.Modal.Builders;
using KvizCommando.Client.Features.Shared.Modal.Components;
using KvizCommando.Client.Features.Solo.Builders;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Features.Home.Builders;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Solo;

public partial class SoloGame : KcComponentBase, IDisposable
{
    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    private readonly Dictionary<string, ContentBoxVm> _boxes = [];

    private string[] _boxOrder = [];
    private SoloGameMode _gameMode;
    private int _selectionId;
    private string _gameTitle = string.Empty;
    private bool _isReady;
    private bool _gameCompleted;
    private int _newTeamLevel;

    private string Culture => AppStates.Culture;
    private SoloGameDtos SoloData => AppStates.SoloGame!;
    private string SelectorCss =>
        _boxOrder.Length > SgameBoxBuilder.Root.Length
            ? "kc-solo-selector-sub"
            : "kc-solo-selector-root";

    protected override void OnInitialized()
    {
        Ui.Header.OnBackBtnClicked += HandleBack;
        Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.GameSolo"], (int)HomeBoxKey.GameSolo);
        _boxOrder = SgameBoxBuilder.Root;
    }

    private ContentBoxVm Box(string key) => _boxes[key];

    private void BuildBoxes()
    {
        var parameters = new SoloComponentParameters
        {
            Mode = _gameMode,
            SelectionId = _selectionId,
            Title = _gameTitle,
            OnGameCompletedChanged = EventCallback.Factory.Create<bool>(
                this,
                SetGameCompleted),
            OnTeamLevelChanged = EventCallback.Factory.Create<int>(
                this,
                SetNewTeamLevel)
        };

        foreach (var box in SgameBoxBuilder.BuildBoxes(
                     SoloData,
                     parameters,
                     Culture,
                     Ui.Lang))
        {
            _boxes[box.Key] = box.Value;
        }

        _isReady = true;
    }

    private void OnBoxClick(int boxId)
    {
        if (boxId is > (int)SgameBoxKeyRoot.Category and <= (int)SgameBoxKeyRoot.Category + SoloBoxSpecs.CATEGORY_BOX_COUNT or > (int)SgameBoxKeyRoot.Orientation and <= (int)SgameBoxKeyRoot.Orientation + SoloBoxSpecs.ORIENTATION_BOX_COUNT)
        {
            _gameCompleted = false;
            _newTeamLevel = 0;
        }

        _boxOrder = SgameBoxBuilder.Root;
        var headerTitle = Ui.Lang["mainlayout.Header.GameSolo"];

        switch (boxId)
        {
            case >= (int)SgameBoxKeyRoot.Category and <= (int)SgameBoxKeyRoot.Category + SoloBoxSpecs.CATEGORY_BOX_COUNT:
                _boxOrder = boxId == (int)SgameBoxKeyRoot.Category ? SgameBoxBuilder.SubCat : SgameBoxBuilder.GameCat;
                _gameMode = SoloGameMode.Category;
                _selectionId = boxId - (int)SgameBoxKeyRoot.Category;
                _gameTitle = boxId == (int)SgameBoxKeyRoot.Category
                    ? string.Empty
                    : _boxes[$"{SgameBoxKeySub.BtnCat}{_selectionId}"].Header;
                headerTitle = _boxes[SgameBoxKeyRoot.Category.ToString()].Header;
                break;

            case >= (int)SgameBoxKeyRoot.Orientation and <= (int)SgameBoxKeyRoot.Orientation + SoloBoxSpecs.ORIENTATION_BOX_COUNT:
                _boxOrder = boxId == (int)SgameBoxKeyRoot.Orientation ? SgameBoxBuilder.SubOri : SgameBoxBuilder.GameOri;
                _gameMode = SoloGameMode.Orientation;
                _selectionId = boxId - (int)SgameBoxKeyRoot.Orientation;
                _gameTitle = boxId == (int)SgameBoxKeyRoot.Orientation
                    ? string.Empty
                    : _boxes[$"{SgameBoxKeySub.BtnOri}{_selectionId}"].Header;
                headerTitle = _boxes[SgameBoxKeyRoot.Orientation.ToString()].Header;
                break;

            case (int)SgameBoxKeyRoot.Campaign:
                headerTitle = _boxes[SgameBoxKeyRoot.Campaign.ToString()].Header;
                break;


        }

        if (boxId is > (int)SgameBoxKeyRoot.Category and <= (int)SgameBoxKeyRoot.Category + SoloBoxSpecs.CATEGORY_BOX_COUNT or > (int)SgameBoxKeyRoot.Orientation and <= (int)SgameBoxKeyRoot.Orientation + SoloBoxSpecs.ORIENTATION_BOX_COUNT)
            BuildBoxes();

        Ui.Header.SetTitle(headerTitle, boxId);
        Ui.Header.SetBackBtnEna(boxId != (int)HomeBoxKey.GameSolo);
        StateHasChanged();
    }

    private async void HandleBack()
    {
        var isActiveGame =
        Ui.Header.PageIndex is > (int)SgameBoxKeyRoot.Category and <= (int)SgameBoxKeyRoot.Category + SoloBoxSpecs.CATEGORY_BOX_COUNT ||
        Ui.Header.PageIndex is > (int)SgameBoxKeyRoot.Orientation and <= (int)SgameBoxKeyRoot.Orientation + SoloBoxSpecs.ORIENTATION_BOX_COUNT;

        if (isActiveGame)
        {
            if (!_gameCompleted)
            {
                var modal = MBoxBuilder.BuildParam(
                    ModalTypes.DialogConfirm,
                    Ui.Lang);

                modal.BodyParameters.Add(
                    nameof(DBoxModalRender.DialogBoxType),
                    DBoxConfirmTypes.SoloGameQuitConfirm);

                if (await Ui.Modal.ShowAsync(modal) != ModalResult.Button1)
                    return;
            }
            else
            {
                if (_newTeamLevel > 0)
                {
                    await Ui.Lang.LoadModuleAsync(Culture, "team");

                    var modal = MBoxBuilder.BuildParam(
                        ModalTypes.TPromoteTeam,
                        Ui.Lang);

                    await Ui.Modal.ShowAsync(modal);
                    _newTeamLevel = 0;
                }

                await Ui.ReloadAsync(
                    ReqStates.Home,
                    ReqStates.Team,
                    ReqStates.SoloGame);
            }
        }
        if (Ui.Header.PageIndex == (int)HomeBoxKey.GameSolo)
        {
            Ui.Nav.NavigateTo("/home");
            return;
        }

        BuildBoxes();
        OnBoxClick((int)HomeBoxKey.GameSolo);
        _gameCompleted = false;
    }

    private void SetGameCompleted(bool value) =>
        _gameCompleted = value;

    private void SetNewTeamLevel(int value) =>
        _newTeamLevel = value;

    /// <inheritdoc />
    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= HandleBack;
        GC.SuppressFinalize(this);
    }
}
