using KvizCommando.Client.Data;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Utilities;
using KvizCommando.Localization.Rankings;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace KvizCommando.Client.Features.Rankings.Components;

/// <summary>A kiválasztott Solo vagy VS ranglista tartalmát jeleníti meg.</summary>
public partial class RankingBoard : KcComponentBase, IDisposable
{
    [Inject] private IStringLocalizer<RankingResource> Lang { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    /// <summary>A tartalmi doboz megnyitásakor kiválasztott lista.</summary>
    [Parameter] public RankingList InitialList { get; set; }

    private RankingList _selectedList;
    private CancellationTokenSource? _refreshCancellation;

    private RankingListDto? Data => _selectedList switch
    {
        RankingList.SoloOverall => AppStates.Ranking?.SoloOverall,
        RankingList.SoloCategory => AppStates.Ranking?.SoloCategory,
        RankingList.SoloOrientation => AppStates.Ranking?.SoloOrientation,
        RankingList.VsAllTime => AppStates.Ranking?.VsAllTime,
        _ => null
    };

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        _selectedList = InitialList;
        _refreshCancellation = new CancellationTokenSource();

        if (AppStates.Ranking is null ||
            (AppStates.Ranking.NextRefreshUtc is DateTime dueUtc &&
             DateTime.UtcNow >= dueUtc))
        {
            await Ui.ReloadAsync(ReqStates.Ranking);
        }

        if (AppStates.Ranking?.NextRefreshUtc is not null)
            _ = RefreshWhileOpenAsync(_refreshCancellation.Token);
    }

    private void SelectList(RankingList list) => _selectedList = list;

    private async Task RefreshWhileOpenAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var snapshot = AppStates.Ranking;
                if (snapshot?.NextRefreshUtc is not DateTime dueUtc)
                    return;

                var wait = dueUtc - DateTime.UtcNow;

                await Task.Delay(
                    wait > TimeSpan.Zero ? wait : TimeSpan.FromSeconds(5),
                    ct);

                try
                {
                    await Ui.ReloadAsync(ReqStates.Ranking);
                }
                catch (HttpRequestException)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), ct);
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
    }

    private string ChoiceClass(RankingList list) =>
        _selectedList == list ? "active" : string.Empty;

    private static string Level(int rank)
    {
        if (rank < 0 || rank >= RankNameTable.Data.Count)
            return string.Empty;

        var level = RankNameTable.Data[rank].PublicLevel;
        return level.StartsWith("Lvl-", StringComparison.Ordinal)
            ? level[4..]
            : level;
    }

    private string Score(double value) =>
        value.ToString(_selectedList == RankingList.VsAllTime ? "0.0" : "0");

    private static string FormatTimeDifference(double difference) =>
        difference > 99.9
            ? "+99.9s"
            : difference.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "s";

    /// <inheritdoc />
    public void Dispose()
    {
        _refreshCancellation?.Cancel();
        _refreshCancellation?.Dispose();
        GC.SuppressFinalize(this);
    }
}
