using BWin2.Wasm.Configuration;
using BWin2.Wasm.State;
using Microsoft.AspNetCore.Components;

namespace BWin2.Wasm.Components;

public partial class GameShell
{
    [Inject] private IGameSession Game { get; set; } = default!;

    private string SeasonLabel =>
        Game.State.CurrentCupRound == 0
            ? $"Bundesliga · {Math.Min(Game.State.Week, 34)}. hét"
            : $"DFB Cup · {GameRules.CupRoundNames[Game.State.CurrentCupRound].Trim()}";

    private void SelectScreen(GameScreen screen)
    {
        Game.SelectScreen(screen);
    }

    private static string ScreenText(GameScreen screen) => screen switch
    {
        GameScreen.Fixtures => "Fixtures",
        GameScreen.LeagueTable => "Tabelle",
        GameScreen.Players => "Players",
        GameScreen.BetSummary => "Bet sum",
        GameScreen.PlayRound => "Match",
        _ => string.Empty
    };
}
