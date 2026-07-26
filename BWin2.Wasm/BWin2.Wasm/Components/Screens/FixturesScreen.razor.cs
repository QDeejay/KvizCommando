using BWin2.Wasm.Configuration;
using BWin2.Wasm.Domain;
using BWin2.Wasm.Services;
using BWin2.Wasm.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BWin2.Wasm.Components.Screens;

public partial class FixturesScreen
{
    private enum FixtureFocus
    {
        Tab,
        Round,
        Odds
    }

    [Inject] private IGameSession Game { get; set; } = default!;

    [Inject] private IScheduleService ScheduleService { get; set; } = default!;

    [Inject] private IOddsService OddsService { get; set; } = default!;

    [Inject] private IBettingService BettingService { get; set; } = default!;

    private ElementReference _host;
    private FixtureFocus _focus;
    private bool _isCup;
    private int _visibleRound;
    private int _selectedFixtureIndex;
    private BetSelection _selectedSelection = BetSelection.Home;
    private bool _isStakeOpen;
    private Fixture? _pendingFixture;
    private BetSelection _pendingSelection;
    private int _stake = GameRules.MinimumStake;
    private string _error = string.Empty;
    private int _lastPayout;

    private GameState State => Game.State;

    private IReadOnlyList<Fixture> Fixtures =>
        _isCup
            ? ScheduleService.GetCupFixtures(State, _visibleRound)
            : ScheduleService.GetLeagueFixtures(State, _visibleRound);

    private string RoundText =>
        _isCup
            ? GameRules.CupRoundNames[_visibleRound].Trim()
            : $"{_visibleRound}. hét";

    private bool ShowOdds =>
        (!_isCup &&
         _visibleRound >= State.Week &&
         _visibleRound < State.Week + 6) ||
        (_isCup &&
         _visibleRound == State.NextCupRound &&
         State.CupRoundScripts[6] == string.Empty);

    private bool CanSelectOdds
    {
        get
        {
            if (State.Credit < GameRules.MinimumStake || !ShowOdds)
                return false;

            if (!_isCup)
                return true;

            return State.CupRoundScripts[_visibleRound + 1] == string.Empty;
        }
    }

    private int MaximumStake =>
        Math.Min(GameRules.MaximumStake, State.Credit);

    private string PendingOddsText
    {
        get
        {
            if (_pendingFixture is not Fixture fixture)
                return string.Empty;

            return GetOdds(fixture).GetText(_pendingSelection);
        }
    }

    private int PendingPotentialWin =>
        Qb.Int(Qb.Val(PendingOddsText) * _stake);

    private string FocusHelp => _focus switch
    {
        FixtureFocus.Tab => "↓ hétválasztás · Enter liga/kupa · → következő menü",
        FixtureFocus.Round => "← → forduló · ↓ oddsok · ↑ Fixtures",
        FixtureFocus.Odds => "↑ ↓ mérkőzés · ← → kimenetel · Enter fogadás",
        _ => string.Empty
    };

    protected override void OnInitialized()
    {
        _isCup = State.CurrentCupRound != 0;
        _visibleRound = _isCup ? State.CurrentCupRound : State.Week;
        _lastPayout = Game.CollectPendingCredit();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await _host.FocusAsync();
    }

    private async Task HandleKeyDown(KeyboardEventArgs args)
    {
        if (_isStakeOpen)
            return;

        switch (_focus)
        {
            case FixtureFocus.Tab:
                if (args.Key == "ArrowRight")
                    Game.NavigateScreen(1);
                else if (args.Key == "ArrowDown")
                    _focus = FixtureFocus.Round;
                else if (args.Key == "Enter")
                    SetCompetition(!_isCup);
                break;

            case FixtureFocus.Round:
                if (args.Key == "ArrowLeft")
                    PreviousRound();
                else if (args.Key == "ArrowRight")
                    NextRound();
                else if (args.Key == "ArrowUp")
                    _focus = FixtureFocus.Tab;
                else if (args.Key == "ArrowDown" && CanSelectOdds)
                {
                    _focus = FixtureFocus.Odds;
                    _selectedFixtureIndex = 0;
                    _selectedSelection = BetSelection.Home;
                }
                break;

            case FixtureFocus.Odds:
                if (args.Key == "ArrowUp")
                {
                    _selectedFixtureIndex--;
                    if (_selectedFixtureIndex < 0)
                    {
                        _selectedFixtureIndex = 0;
                        _focus = FixtureFocus.Round;
                    }
                }
                else if (args.Key == "ArrowDown")
                {
                    _selectedFixtureIndex++;
                    if (_selectedFixtureIndex >= Fixtures.Count)
                    {
                        _selectedFixtureIndex = 0;
                        _focus = FixtureFocus.Tab;
                    }
                }
                else if (args.Key == "ArrowLeft")
                    MoveSelection(-1);
                else if (args.Key == "ArrowRight")
                    MoveSelection(1);
                else if (args.Key == "Enter" && Fixtures.Count > 0)
                    OpenStake(Fixtures[_selectedFixtureIndex], _selectedSelection);
                break;
        }

        await InvokeAsync(StateHasChanged);
    }

    private void SetCompetition(bool cup)
    {
        _isCup = cup;
        _visibleRound = cup ? State.NextCupRound : State.Week;
        _selectedFixtureIndex = 0;
        _selectedSelection = BetSelection.Home;
    }

    private void PreviousRound()
    {
        _visibleRound = Math.Max(_visibleRound - 1, 1);
    }

    private void NextRound()
    {
        int maximum = _isCup
            ? Math.Max(State.NextCupRound, 1)
            : GameRules.LeagueRoundCount;
        _visibleRound = Math.Min(_visibleRound + 1, maximum);
    }

    private void MoveSelection(int direction)
    {
        BetSelection[] selections = _isCup
            ? [BetSelection.Home, BetSelection.Away]
            : [BetSelection.Home, BetSelection.Draw, BetSelection.Away];
        int index = Array.IndexOf(selections, _selectedSelection);
        index = (index + direction + selections.Length) % selections.Length;
        _selectedSelection = selections[index];
    }

    private MatchOdds GetOdds(Fixture fixture) =>
        OddsService.CalculateMatchOdds(
            State.TeamAt(fixture.HomeTeamSlot),
            State.TeamAt(fixture.AwayTeamSlot),
            _isCup,
            _isCup ? State.NextCupRound : 0);

    private IEnumerable<(BetSelection Selection, string Text)> DisplayedOdds(
        MatchOdds odds)
    {
        yield return (BetSelection.Home, odds.GetText(BetSelection.Home));
        if (!_isCup)
            yield return (BetSelection.Draw, odds.GetText(BetSelection.Draw));
        yield return (BetSelection.Away, odds.GetText(BetSelection.Away));
    }

    private IReadOnlyList<Bet> GetFixtureBets(int matchNumber) =>
        State.Bets.Where(bet =>
                bet.EventKind ==
                    (_isCup ? BetEventKind.CupMatch : BetEventKind.LeagueMatch) &&
                (_isCup
                    ? bet.CupRound == _visibleRound
                    : bet.EventWeek == _visibleRound) &&
                bet.MatchNumber == matchNumber)
            .ToList();

    private string FixtureRowClass(Fixture fixture) =>
        _focus == FixtureFocus.Odds &&
        Fixtures[_selectedFixtureIndex].MatchNumber == fixture.MatchNumber
            ? "selected"
            : string.Empty;

    private string SelectedTeamClass(int teamSlot)
    {
        int selected = _isCup
            ? State.CupWinnerBetTeamSlot
            : State.ChampionBetTeamSlot;
        return teamSlot == selected ? "selected-team" : string.Empty;
    }

    private string OddsClass(
        Fixture fixture,
        BetSelection selection,
        Bet? placedBet)
    {
        var classes = new List<string>();
        if (_focus == FixtureFocus.Odds &&
            Fixtures[_selectedFixtureIndex].MatchNumber == fixture.MatchNumber &&
            _selectedSelection == selection)
        {
            classes.Add("selected");
        }

        if (placedBet is not null)
            classes.Add("placed");

        return string.Join(" ", classes);
    }

    private string OddsMovement(MatchOdds odds, Bet bet)
    {
        double difference =
            Qb.Val(odds.GetText(bet.Selection)) -
            Qb.Val(bet.OddsText);
        if (Math.Abs(difference) < .0001)
            return "±0.00";

        return difference > 0
            ? $"+{Math.Abs(difference):0.00}"
            : $"-{Math.Abs(difference):0.00}";
    }

    private void ChooseOdds(Fixture fixture, BetSelection selection)
    {
        _focus = FixtureFocus.Odds;
        _selectedFixtureIndex = Fixtures
            .Select((item, index) => (item, index))
            .First(pair => pair.item.MatchNumber == fixture.MatchNumber)
            .index;
        _selectedSelection = selection;
        OpenStake(fixture, selection);
    }

    private void OpenStake(Fixture fixture, BetSelection selection)
    {
        if (!CanSelectOdds ||
            BettingService.HasOpenMatchBet(
                State,
                _isCup,
                _visibleRound,
                fixture.MatchNumber))
        {
            return;
        }

        _pendingFixture = fixture;
        _pendingSelection = selection;
        _stake = GameRules.MinimumStake;
        _error = string.Empty;
        _isStakeOpen = true;
    }

    private void CancelStake()
    {
        _isStakeOpen = false;
        _pendingFixture = null;
        _error = string.Empty;
    }

    private void ConfirmStake()
    {
        if (_pendingFixture is not Fixture fixture)
            return;

        if (!BettingService.IsStakeValid(_stake, State.Credit))
        {
            _error =
                $"A tét {GameRules.MinimumStake:N0} és {MaximumStake:N0} HUF között lehet.";
            return;
        }

        BettingService.PlaceMatchBet(
            State,
            fixture,
            _isCup,
            _visibleRound,
            _pendingSelection,
            GetOdds(fixture),
            _stake);
        _isStakeOpen = false;
        _pendingFixture = null;
        _focus = FixtureFocus.Tab;
    }

    private string FocusClass(FixtureFocus focus) =>
        _focus == focus ? "focus-ring" : string.Empty;

    private static string SelectionText(BetSelection selection) =>
        selection switch
        {
            BetSelection.Home => "Home",
            BetSelection.Draw => "Draw",
            BetSelection.Away => "Away",
            _ => string.Empty
        };
}
