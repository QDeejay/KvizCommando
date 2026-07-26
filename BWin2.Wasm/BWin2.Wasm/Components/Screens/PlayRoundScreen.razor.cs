using BWin2.Wasm.Configuration;
using BWin2.Wasm.Domain;
using BWin2.Wasm.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BWin2.Wasm.Components.Screens;

public partial class PlayRoundScreen
{
    [Inject] private IGameSession Game { get; set; } = default!;

    private ElementReference _host;
    private bool _showBetMatches;

    private bool HasBetMatch => Game.HasVisibleBetMatch();

    private string CurrentRoundText =>
        Game.State.CurrentCupRound == 0
            ? $"{Game.State.Week}. hét"
            : GameRules.CupRoundNames[Game.State.CurrentCupRound].Trim();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await _host.FocusAsync();
    }

    private async Task HandleKeyDown(KeyboardEventArgs args)
    {
        if (Game.IsRoundRunning || Game.LastRound is not null)
            return;

        if (args.Key == "ArrowLeft")
            Game.NavigateScreen(-1);
        else if (HasBetMatch && args.Key is "ArrowUp" or "ArrowDown")
            _showBetMatches = !_showBetMatches;
        else if (args.Key == "Enter")
        {
            if (Game.IsSeasonFinished())
                Game.BeginNewSeason();
            else
                await PlayRoundAsync();
        }
    }

    private void SelectOption(bool showBetMatches)
    {
        _showBetMatches = showBetMatches && HasBetMatch;
    }

    private async Task PlayRoundAsync()
    {
        await Game.PlayRoundAsync(_showBetMatches && HasBetMatch);
    }

    private IEnumerable<Bet> MatchBets(PlayedMatch match)
    {
        bool cup = Game.State.CurrentCupRound != 0;
        int round = cup
            ? Game.State.CurrentCupRound
            : Game.State.Week;

        return Game.State.Bets.Where(bet =>
            bet.EventKind ==
                (cup ? BetEventKind.CupMatch : BetEventKind.LeagueMatch) &&
            (cup ? bet.CupRound == round : bet.EventWeek == round) &&
            bet.MatchNumber == match.Fixture.MatchNumber);
    }

    private void CompleteRound()
    {
        Game.CompleteRound();
    }

    private void BeginNewSeason()
    {
        Game.BeginNewSeason();
    }
}
