using BWin2.Wasm.Domain;
using BWin2.Wasm.Services;
using BWin2.Wasm.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BWin2.Wasm.Components.Screens;

public partial class BetSummaryScreen
{
    [Inject] private IGameSession Game { get; set; } = default!;

    [Inject] private IBettingService BettingService { get; set; } = default!;

    private ElementReference _host;

    protected override void OnInitialized()
    {
        BettingService.RemoveOldMatchBets(Game.State);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await _host.FocusAsync();
    }

    private void HandleKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "ArrowLeft")
            Game.NavigateScreen(-1);
        else if (args.Key == "ArrowRight")
            Game.NavigateScreen(1);
        else if (args.Key is " " or "Spacebar")
            Game.SelectScreen(GameScreen.Fixtures);
    }

    private static string BetClass(Bet bet)
    {
        if (bet.Status == BetStatus.Opened)
            return "open";
        return bet.Lost ? "lost" : "won";
    }

    private static string WinText(Bet bet) =>
        bet.Lost ? "Lost" : $"{bet.PotentialWin:N0}";
}
