using BWin2.Wasm.Domain;

namespace BWin2.Wasm.State;

internal interface IGameSession
{
    event Action? Changed;

    GameState State { get; }

    GamePhase Phase { get; }

    GameScreen CurrentScreen { get; }

    BetEventKind PreseasonBetKind { get; }

    RoundPlayResult? LastRound { get; }

    bool IsRoundRunning { get; }

    string? ErrorMessage { get; }

    Task InitializeAsync(CancellationToken ct = default);

    bool PlacePreseasonBet(int teamSlot, int stake);

    void SelectScreen(GameScreen screen);

    void NavigateScreen(int direction);

    bool IsSeasonFinished();

    bool HasVisibleBetMatch();

    Task PlayRoundAsync(bool showBetMatches, CancellationToken ct = default);

    void CompleteRound();

    void BeginNewSeason();

    int CollectPendingCredit();
}
