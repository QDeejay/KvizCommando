using KvizCommando.Client.Features.VsGame.Builders;
using KvizCommando.Client.Features.VsGame.Match.Services;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.VsGame;

public partial class VsGame : KcComponentBase, IDisposable
{
    [Inject]
    private IVsMatchClientService MatchClient { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    private readonly Dictionary<string, ContentBoxVm> _boxes = [];

    private string[] _boxOrder = [];
    private int _classificationId;
    private bool _isMatchLocked;
    private bool _isLeavingMatch;
    private bool _isReady;

    private string Culture => AppStates.Culture;
    private VsGameDtos VsData => AppStates.VsGame!;

    protected override void OnInitialized()
    {
        Ui.Header.OnBackBtnClicked += HandleBack;
        Ui.Header.SetTitle(
            Ui.Lang["mainlayout.Header.GameVs"],
            3);
        _boxOrder = VsBoxBuilder.Root;
    }

    private ContentBoxVm Box(string key) => _boxes[key];

    private void BuildBoxes()
    {
        var parameters = new VsComponentParameters
        {
            OnTeamSaved = RefreshRankedAsync,
            ClassificationId = _classificationId,
            OnMatchLockChanged = SetMatchLockedAsync
        };

        foreach (var box in VsBoxBuilder.BuildBoxes(
                     VsData,
                     parameters,
                     Ui.Lang))
        {
            _boxes[box.Key] = box.Value;
        }

        _isReady = true;
    }

    private void OnBoxClick(int boxId)
    {
        if (_isMatchLocked)
            return;

        _boxOrder = VsBoxBuilder.Root;
        var headerTitle =
            Ui.Lang["mainlayout.Header.GameVs"];

        if (boxId == 303)
        {
            _boxOrder = VsBoxBuilder.Ranked;
            headerTitle = _boxes[
                VsBoxKeyRoot.RtBtnRankedBattlefields
                    .ToString()].Header;
        }
        else if (boxId is >= 311 and <= 315)
        {
            _classificationId = boxId - 310;
            BuildBoxes();
            _boxOrder = VsBoxBuilder.Match;
            headerTitle = _boxes[
                $"{VsBoxKeyRanked.Classification}" +
                $"{_classificationId}"].Header;
        }

        Ui.Header.SetTitle(headerTitle, boxId);
        Ui.Header.SetBackBtnEna(boxId != 3);
        StateHasChanged();
    }

    private Task RefreshRankedAsync()
    {
        BuildBoxes();
        OnBoxClick(303);
        return Task.CompletedTask;
    }

    private Task SetMatchLockedAsync(bool isLocked)
    {
        _isMatchLocked = isLocked;
        Ui.Header.SetBackBtnEna(true);
        return Task.CompletedTask;
    }

    private void HandleBack()
    {
        if (Ui.Header.PageIndex is >= 311 and <= 315)
        {
            _ = LeaveMatchAsync();
            return;
        }

        if (Ui.Header.PageIndex == 3)
        {
            Ui.Nav.NavigateTo("/home");
            return;
        }

        BuildBoxes();
        OnBoxClick(3);
    }

    private async Task LeaveMatchAsync()
    {
        if (_isLeavingMatch)
            return;

        _isLeavingMatch = true;

        try
        {
            await MatchClient.StopAsync();
            _isMatchLocked = false;
            BuildBoxes();
            OnBoxClick(303);
        }
        finally
        {
            _isLeavingMatch = false;
        }
    }

    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= HandleBack;
        GC.SuppressFinalize(this);
    }
}

/**
 * MÓDOSÍTÁS: a rangbesorolás kiválasztásakor a VS lap a
 * DynamicComponent meccsmanagerre vált. A fejléc vissza gombja lock
 * után is engedélyezett hivatalos kilépés: előbb lezárja a SignalR
 * kapcsolatot, így a szerver OnDisconnected ága elvégzi a
 * queue/match takarítását, és csak utána vált vissza a ranked menüre.
 *
 * A fájl a VS menü dobozsorrendjét és navigációs állapotát kezeli.
 */
