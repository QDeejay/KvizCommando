using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Solo.ViewModels;
using KvizCommando.Localization.SoloGame;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace KvizCommando.Client.Features.Solo.Components;

public partial class SoloRewardView
{
    [Inject]
    private IStringLocalizer<SoloPlayResource> Lang { get; set; } = default!;

    [Parameter, EditorRequired]
    public SoloRewardViewData Data { get; set; } = default!;

    private string NewTeamLevelCode =>
        RankNameTable.Data[Data.NewTeamLevel].PublicLevel ??
        string.Empty;

    private static string Signed(int value) =>
        $"{(value > 0 ? "+" : string.Empty)}{value}";
}
