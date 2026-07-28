using KvizCommando.Client.Features.VsGame.Builders;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.VsGame;

public partial class VsGame : KcComponentBase, IDisposable
{
    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    private readonly Dictionary<string, ContentBoxVm> _boxes = [];

    private string[] _boxOrder = [];
    private int _classificationId;
    private bool _isMatchLocked;
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
        Ui.Header.SetBackBtnEna(!isLocked);
        return Task.CompletedTask;
    }

    private void HandleBack()
    {
        if (_isMatchLocked)
            return;

        if (Ui.Header.PageIndex == 3)
        {
            Ui.Nav.NavigateTo("/home");
            return;
        }

        var returnToRanked =
            Ui.Header.PageIndex is >= 311 and <= 315;

        BuildBoxes();
        OnBoxClick(returnToRanked ? 303 : 3);
    }

    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= HandleBack;
        GC.SuppressFinalize(this);
    }
}

/**
 * MÓDOSÍTÁS: a rangbesorolás kiválasztásakor a VS lap a
 * DynamicComponent meccsmanagerre vált, MatchLocked után pedig
 * letiltja a visszalépést. Lock előtt a visszalépés büntetlenül
 * megszünteti a várólistás kapcsolatot.
 *
 * A fájl a VS menü dobozsorrendjét és navigációs állapotát kezeli.
 */
