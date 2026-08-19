using KvizCommando.Client.Data;
using KvizCommando.Client.Features.VsGame.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Client.Features.VsGame.Builders;

/// <summary>
/// A VS várólista, meccs, előkészítés és jutalom kliensoldali nézetadatait állítja össze.
/// </summary>
public sealed partial class VsMatchViewBuilder
{
private const string CATEGORY_IMAGE_ROOT =
        "images/buttons/solo/categories";

    private static readonly string[] CategoryFileNames =
    [
        "",
        "religion",
        "famousdates",
        "music",
        "sport",
        "technology",
        "naturalscience",
        "famouspepole",
        "sculpture_painting",
        "mythology",
        "history",
        "movies",
        "game",
        "it",
        "geo_astro",
        "fashion",
        "literature"
    ];

    private readonly ILanguageService _lang;

    /// <summary>
    /// Létrehozza a VS nézetadatokat összeállító buildert.
    /// </summary>
    /// <param name="lang">A feliratok feloldásához használt nyelvi szolgáltatás.</param>
    public VsMatchViewBuilder(ILanguageService lang)
    {
        _lang = lang;
    }

    /// <summary>
    /// Összeállítja a rangsorolt várólista nézetadatait.
    /// </summary>
    /// <param name="snapshot">A kliensnek továbbítandó aktuális állapotpillanatkép.</param>
    public VsQueueViewData BuildQueue(
        VsRankedQueueSnapshot snapshot)
    {
        return new VsQueueViewData
        {
            ClassificationText =
                _lang[
                    $"vsgame.Classification.Title.{snapshot.ClassificationId}"],
            StatusText = _lang["vsgame.Match.Queue.Status"],
            WaitingPlayers = snapshot.WaitingPlayers,
            RequiredPlayers = snapshot.RequiredPlayers,
            RequiredPartySize = snapshot.RequiredPartySize,
            Stake = snapshot.Stake,
            MatchmakingDeadlineUtc =
                snapshot.MatchmakingDeadlineUtc,
            Players =
            [
                .. snapshot.Players.Select(player =>
                    BuildPlayer(player))
            ]
        };
    }

    /// <summary>
    /// Összeállítja a bemeneti adatokhoz tartozó megjelenítési modellt.
    /// </summary>
    /// <param name="snapshot">A kliensnek továbbítandó aktuális állapotpillanatkép.</param>
    /// <param name="culture">A kért kultúra neve, például <c>hu-HU</c>.</param>
    public VsMatchViewData Build(
        VsMatchSnapshot snapshot,
        string culture)
    {
        return new VsMatchViewData
        {
            MatchId = snapshot.MatchId,
            Phase = snapshot.Phase,
            DeadlineUtc = snapshot.DeadlineUtc,
            PhaseDurationSeconds =
                snapshot.PhaseDurationSeconds,
            InfoText = _lang[snapshot.InfoKey],
            ClassificationText =
                _lang[
                    $"vsgame.Classification.Title.{snapshot.ClassificationId}"],
            Stake = snapshot.Stake,
            Players =
            [
                .. snapshot.Players.Select(player =>
                    BuildPlayer(player, culture))
            ],
            Preparation = BuildPreparation(
                snapshot.Preparation,
                culture),
            Game = BuildGame(snapshot.Game, culture),
            Reward = BuildReward(snapshot.Reward)
        };
    }
}
