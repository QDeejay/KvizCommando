using KvizCommando.Client.Features.Shared.Modal.Builders;
using KvizCommando.Client.Features.Shared.Modal.Components;
using KvizCommando.Client.Features.Solo.Builders;
using KvizCommando.Client.Models.ViewModels;
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
        Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.GameSolo"], 4);
        _boxOrder = SgameBoxBuilder.Root;
    }

    private ContentBoxVm Box(string key) => _boxes[key];

    private void BuildBoxes()
    {
        foreach (var box in SgameBoxBuilder.BuildBoxes(
                     SoloData,
                     Culture,
                     Ui.Lang))
        {
            _boxes[box.Key] = box.Value;
        }

        _isReady = true;
    }

    private void OnBoxClick(int boxId)
    {
        if (boxId is >= 421 and <= 436 or >= 451 and <= 458)
        {
            _gameCompleted = false;
            _newTeamLevel = 0;
        }

        _boxOrder = SgameBoxBuilder.Root;
        var headerTitle = Ui.Lang["mainlayout.Header.GameSolo"];

        switch (boxId)
        {
            case >= 420 and <= 436:
                _boxOrder = boxId == 420 ? SgameBoxBuilder.SubCat : SgameBoxBuilder.GameCat;
                _gameMode = SoloGameMode.Category;
                _selectionId = boxId - 420;
                headerTitle = _boxes[SgameBoxKeyRoot.RtBtnCategory.ToString()].Header;
                break;

            case >= 450 and <= 458:
                _boxOrder = boxId == 450 ? SgameBoxBuilder.SubOri : SgameBoxBuilder.GameOri;
                _gameMode = SoloGameMode.Orientation;
                _selectionId = boxId - 450;
                headerTitle = _boxes[SgameBoxKeyRoot.RtBtnOrient.ToString()].Header;
                break;

            case 403:
                headerTitle = _boxes[SgameBoxKeyRoot.RtBtnCampaign.ToString()].Header;
                break;


        }

        Ui.Header.SetTitle(headerTitle, boxId);
        Ui.Header.SetBackBtnEna(boxId != 4);
        StateHasChanged();
    }

    private async void HandleBack()
    {
        var isActiveGame =
        Ui.Header.PageIndex is >= 421 and <= 436 ||
        Ui.Header.PageIndex is >= 451 and <= 458;

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
            else if (_newTeamLevel > 0)
            {
                await Ui.Lang.LoadModuleAsync(Culture, "team");

                var modal = MBoxBuilder.BuildParam(
                    ModalTypes.TPromoteTeam,
                    Ui.Lang);

                modal.BodyParameters.Add(
                    nameof(TModalRender.AchievedTeamLevel),
                    _newTeamLevel);

                await Ui.Modal.ShowAsync(modal);
                _newTeamLevel = 0;
            }
        }
        if (Ui.Header.PageIndex == 4)
        {
            Ui.Nav.NavigateTo("/home");
            return;
        }

        BuildBoxes();
        OnBoxClick(4);
        _gameCompleted = false;
    }

    private void SetGameCompleted(bool value) =>
        _gameCompleted = value;

    private void SetNewTeamLevel(int value) =>
        _newTeamLevel = value;

    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= HandleBack;
        GC.SuppressFinalize(this);
    }
}

/**
 * MÓDOSÍTÁS: a Solo oldal külön kezeli a folyamatban lévő és a már
 * befejezett játék visszalépését. Tényleges csapatszintlépés után a
 * meglévő csapat-előléptetési modalt mutatja, majd folytatja a kilépést.
 */
