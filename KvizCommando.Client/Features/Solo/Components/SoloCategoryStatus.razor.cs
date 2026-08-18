using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Rules;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Solo.Components;

public partial class SoloCategoryStatus
{
    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    [Parameter]
    public int CategoryPosition { get; set; }

    private ResultDto Result =>
        AppStates.SoloGame!.Results.CategoryResults[CategoryPosition];

    private int TeamLevel =>
        AppStates.Home!.UserMainData.RankEnum;

    private string MaxedCssClass =>
        SoloGameRules.HasMaxedScoreDevelopmentPoints(
            Result.Points,
            TeamLevel)
                ? "maxed"
                : string.Empty;

    private string MaximumScoreCssClass =>
        Result.Points >= SoloGameRules.GetMaximumScore(TeamLevel)
            ? "maximum-score"
            : string.Empty;
}
