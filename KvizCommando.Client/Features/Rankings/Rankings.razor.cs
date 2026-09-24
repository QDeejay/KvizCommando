using KvizCommando.Client.Features.Home.Builders;
using KvizCommando.Client.Features.Rankings.Builders;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Utilities;
using KvizCommando.Localization.Rankings;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace KvizCommando.Client.Features.Rankings;

/// <summary>A ranglisták gyökérképernyőjét és tartalmi dobozait kezeli.</summary>
public partial class Rankings : KcComponentBase, IDisposable
{
    [Inject] private IStringLocalizer<RankingResource> Lang { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    private Dictionary<string, ContentBoxVm> _boxes = [];
    private string[] _boxOrder = [];

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        Ui.Header.OnBackBtnClicked += HandleBack;
        Ui.Header.SetTitle(
            AppStates.BoxTitles["Root.Rankings"],
            (int)HomeBoxKey.Rankings);
        Ui.Header.SetBackBtnEna(false);
        _boxOrder = RankingBoxBuilder.Root;
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        _boxes = RankingBoxBuilder.BuildBoxes(
            AppStates.BoxTitles, AppStates.Ranking, Lang);
    }

    private void OpenSection(int boxId)
    {
        var section = (RankingBoxKey)boxId;
        if (section is not (RankingBoxKey.Solo or RankingBoxKey.Vs))
            return;

        _boxOrder = section == RankingBoxKey.Solo
            ? RankingBoxBuilder.Solo
            : RankingBoxBuilder.Vs;
        Ui.Header.SetTitle(_boxes[section.ToString()].Header, boxId);
        Ui.Header.SetBackBtnEna(true);
    }

    private void HandleBack()
    {
        if (ReferenceEquals(_boxOrder, RankingBoxBuilder.Root))
        {
            Ui.Nav.NavigateTo("/home");
            return;
        }

        _boxOrder = RankingBoxBuilder.Root;
        Ui.Header.SetTitle(
            AppStates.BoxTitles["Root.Rankings"],
            (int)HomeBoxKey.Rankings);
        Ui.Header.SetBackBtnEna(false);
        StateHasChanged();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= HandleBack;
        GC.SuppressFinalize(this);
    }
}
