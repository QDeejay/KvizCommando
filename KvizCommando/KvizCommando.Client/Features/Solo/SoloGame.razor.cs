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
        _boxOrder = SgameBoxBuilder.Root;
        var headerTitle = Ui.Lang["mainlayout.Header.GameSolo"];

        switch (boxId)
        {
            case >= 420 and <= 436:
                _boxOrder = boxId==420 ? SgameBoxBuilder.SubCat : SgameBoxBuilder.GameCat;
                _gameMode = SoloGameMode.Category;
                _selectionId = boxId - 420;
                 headerTitle = _boxes[ SgameBoxKeyRoot.RtBtnCategory.ToString()].Header;
                break;

            case >= 450 and <= 458:
                _boxOrder = boxId==450 ? SgameBoxBuilder.SubOri : SgameBoxBuilder.GameOri;
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
