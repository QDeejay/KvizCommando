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
            OnTeamSaved = RefreshRankedAsync
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
        if (boxId is >= 611 and <= 615)
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

    private void HandleBack()
    {
        if (Ui.Header.PageIndex == 3)
        {
            Ui.Nav.NavigateTo("/home");
            return;
        }

        BuildBoxes();
        OnBoxClick(3);
    }

    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= HandleBack;
        GC.SuppressFinalize(this);
    }
}
