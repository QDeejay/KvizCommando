using KvizCommando.Client.Data;
using KvizCommando.Client.Features.VsGame.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.VsGame.Components;

public partial class VsMatchRewardView
{
    [Inject]
    private ILanguageService Lang { get; set; } = default!;

    [Parameter, EditorRequired]
    public VsMatchViewData Data { get; set; } = default!;

    private VsMyRewardVm MyReward => Data.Reward.MyReward!;

    private int PensionTotal =>
        MyReward.Characters.Sum(character => character.Pension);

    private int NewTeamLevel => MyReward.NewTeamLevel;

    private string NewTeamLevelCode =>
        RankNameTable.Data[NewTeamLevel].PublicLevel ??
        string.Empty;

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
                    help.Count > 1
                        ? $"{help.Name} ×{help.Count}"
                        : help.Name));

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
 * újra és nem indít kliensoldali mentést. A segítségeket levonási
 * előjel nélkül, többszörös előfordulásnál darabszámmal formázza.
 * MÓDOSÍTÁS: a karakterenként már kiszámolt nyugdíjalap-jóváírások
 * megjelenített összegét adja vissza; rewardot nem számol újra.
 * MÓDOSÍTÁS: a szintlépés-animációt a szerver tényleges NewTeamLevel
 * eredménye vezérli, amelyből a komponens csak a PublicLevel kódot oldja fel.
 */
