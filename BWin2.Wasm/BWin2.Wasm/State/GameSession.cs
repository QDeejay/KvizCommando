using BWin2.Wasm.Configuration;
using BWin2.Wasm.Data;
using BWin2.Wasm.Domain;
using BWin2.Wasm.Services;

namespace BWin2.Wasm.State;

internal sealed class GameSession : IGameSession
{
    private readonly IGameDataStore _dataStore;
    private readonly IScheduleService _scheduleService;
    private readonly IOddsService _oddsService;
    private readonly IBettingService _bettingService;
    private readonly ILeagueService _leagueService;
    private readonly ISeasonProgressService _seasonProgress;
    private readonly IMatchEngine _matchEngine;

    private GameState? _state;

    public GameSession(
        IGameDataStore dataStore,
        IScheduleService scheduleService,
        IOddsService oddsService,
        IBettingService bettingService,
        ILeagueService leagueService,
        ISeasonProgressService seasonProgress,
        IMatchEngine matchEngine)
    {
        _dataStore = dataStore;
        _scheduleService = scheduleService;
        _oddsService = oddsService;
        _bettingService = bettingService;
        _leagueService = leagueService;
        _seasonProgress = seasonProgress;
        _matchEngine = matchEngine;
    }

    public event Action? Changed;

    public GameState State =>
        _state ?? throw new InvalidOperationException("The game is not loaded.");

    public GamePhase Phase { get; private set; } = GamePhase.Loading;

    public GameScreen CurrentScreen { get; private set; } = GameScreen.Fixtures;

    public BetEventKind PreseasonBetKind { get; private set; } =
        BetEventKind.Champion;

    public RoundPlayResult? LastRound { get; private set; }

    public bool IsRoundRunning { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        try
        {
            _state = await _dataStore.LoadAsync(ct);
            _scheduleService.InitializeFormAdjustments(State);
            Phase = GamePhase.Preseason;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Phase = GamePhase.Failed;
        }

        NotifyChanged();
    }

    public bool PlacePreseasonBet(int teamSlot, int stake)
    {
        if (Phase != GamePhase.Preseason ||
            !_bettingService.IsStakeValid(stake, State.Credit))
        {
            return false;
        }

        bool cupWinner = PreseasonBetKind == BetEventKind.CupWinner;
        string odds = _oddsService.CalculatePreseasonOdds(
            State.TeamAt(1),
            State.TeamAt(teamSlot),
            cupWinner);

        _bettingService.PlaceSeasonBet(
            State,
            PreseasonBetKind,
            teamSlot,
            odds,
            stake);

        if (!cupWinner)
        {
            State.ChampionBetTeamSlot = teamSlot;
            PreseasonBetKind = BetEventKind.CupWinner;
        }
        else
        {
            State.CupWinnerBetTeamSlot = teamSlot;
            _scheduleService.GenerateSeason(State);
            Phase = GamePhase.Main;
            CurrentScreen = GameScreen.Fixtures;
        }

        NotifyChanged();
        return true;
    }

    public void SelectScreen(GameScreen screen)
    {
        if (Phase != GamePhase.Main || IsRoundRunning)
            return;

        CurrentScreen = screen;
        NotifyChanged();
    }

    public void NavigateScreen(int direction)
    {
        int screen = Math.Clamp(
            (int)CurrentScreen + Math.Sign(direction),
            (int)GameScreen.Fixtures,
            (int)GameScreen.PlayRound);
        SelectScreen((GameScreen)screen);
    }

    public bool IsSeasonFinished() =>
        State.Week >= 35 &&
        State.CupRoundScripts[GameRules.CupRoundCount + 1] != string.Empty;

    public bool HasVisibleBetMatch()
    {
        if (IsSeasonFinished())
            return false;

        bool cup = State.CurrentCupRound != 0;
        int round = cup ? State.CurrentCupRound : State.Week;
        IReadOnlyList<Fixture> fixtures = cup
            ? _scheduleService.GetCupFixtures(State, round)
            : _scheduleService.GetLeagueFixtures(State, round);

        return fixtures.Any(fixture =>
            _bettingService.IsMatchVisibleBecauseOfBet(
                State,
                cup,
                round,
                fixture.MatchNumber));
    }

    public async Task PlayRoundAsync(
        bool showBetMatches,
        CancellationToken ct = default)
    {
        if (IsRoundRunning || IsSeasonFinished())
            return;

        State.HasStarted = true;
        IsRoundRunning = true;
        LastRound = null;
        NotifyChanged();

        try
        {
            LastRound = await _matchEngine.PlayRoundAsync(
                State,
                showBetMatches,
                ct);
        }
        finally
        {
            IsRoundRunning = false;
            NotifyChanged();
        }
    }

    public void CompleteRound()
    {
        if (LastRound is null)
            return;

        _seasonProgress.AdvanceAfterRound(State);
        LastRound = null;
        CurrentScreen = GameScreen.Fixtures;
        NotifyChanged();
    }

    public void BeginNewSeason()
    {
        if (!IsSeasonFinished())
            return;

        _leagueService.PrepareNewSeason(State);
        PreseasonBetKind = BetEventKind.Champion;
        LastRound = null;
        Phase = GamePhase.Preseason;
        CurrentScreen = GameScreen.Fixtures;
        NotifyChanged();
    }

    public int CollectPendingCredit()
    {
        int payout = State.PendingCredit;
        if (payout == 0)
            return 0;

        State.Credit += payout;
        State.PendingCredit = 0;
        NotifyChanged();
        return payout;
    }

    private void NotifyChanged() => Changed?.Invoke();
}
