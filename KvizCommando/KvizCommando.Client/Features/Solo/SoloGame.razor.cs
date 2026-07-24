using KvizCommando.Client.Features.Solo.Builders;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
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
        if (boxId is >= 421 and <= 436)
        {
            BeginGame(SoloGameMode.Category, boxId - 420, boxId);
            return;
        }

        if (boxId is >= 451 and <= 458)
        {
            BeginGame(SoloGameMode.Orientation, boxId - 450, boxId);
            return;
        }

        _boxOrder = SgameBoxBuilder.Root;
        var headerTitle = Ui.Lang["mainlayout.Header.GameSolo"];

        switch (boxId)
        {
            case 401:
                _boxOrder = SgameBoxBuilder.SubCat;
                headerTitle = _boxes[
                    SgameBoxKeyRoot.RtBtnCategory.ToString()].Header;
                break;

            case 402:
                _boxOrder = SgameBoxBuilder.SubOri;
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

    private void BeginGame(
        SoloGameMode mode,
        int selectionId,
        int boxId)
    {
        _gameMode = mode;
        _selectionId = selectionId;
        _gameTitle = mode == SoloGameMode.Category
            ? _boxes[$"{SgameBoxKeySub.BtnCat}{selectionId}"].Header
            : _boxes[$"{SgameBoxKeySub.BtnOri}{selectionId}"].Header;

        _boxOrder = mode == SoloGameMode.Category
            ? SgameBoxBuilder.GameCat
            : SgameBoxBuilder.GameOri;


        Ui.Header.SetTitle(Ui.Header.Title, boxId);
        Ui.Header.SetBackBtnEna(true);
        StateHasChanged();
    }

    private void HandleBack()
    {
        if (Ui.Header.PageIndex == 4)
        {
            Ui.Nav.NavigateTo("/home");
            return;
        }

        BuildBoxes();
        OnBoxClick(4);
    }

    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= HandleBack;
        GC.SuppressFinalize(this);
    }
}
