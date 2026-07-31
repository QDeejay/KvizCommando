using KvizCommando.Client.Features.VsGame.Match.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.VsGame.Match.Components;

public partial class VsMatchRewardView
{
    [Inject]
    private ILanguageService Lang { get; set; } = default!;

    [Parameter, EditorRequired]
    public VsMatchViewData Data { get; set; } = default!;

    private VsMyRewardVm MyReward => Data.Reward.MyReward!;

    private VsRewardStandingVm Winner =>
        Data.Reward.Standings.First(player => player.IsWinner);

    private string MatchReference =>
        Data.MatchId.ToString("N")[..8].ToUpperInvariant();

    private string ConsumedHelpsText =>
        MyReward.ConsumedHelps.Length == 0
            ? Lang["vsgame.Match.Reward.None"]
            : string.Join(
                ", ",
                MyReward.ConsumedHelps.Select(help =>
                    $"-{help.Count} {help.Name}"));

    private static string Signed(int value) =>
        $"{(value > 0 ? "+" : string.Empty)}{value}";

    private string Credits(int value) =>
        $"{Signed(value)} {Lang["vsgame.Match.Reward.CreditUnit"]}";

    private string Energy(int loss) =>
        $"-{loss} {Lang["vsgame.Match.Game.EnergyShort"]}";

    private static string Seconds(double value) =>
        $"{value:0.0}s";
}

/**
 * ÚJ FÁJL: a jutalomkomponens paraméterét és kizárólag formázási
 * segédeit tartalmazza. A szerveren számolt értékeket nem számolja
 * újra és nem indít kliensoldali mentést.
 */
