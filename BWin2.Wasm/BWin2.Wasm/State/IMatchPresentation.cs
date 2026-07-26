using BWin2.Wasm.Domain;

namespace BWin2.Wasm.State;

internal interface IMatchPresentation
{
    event Action? Changed;

    bool IsActive { get; }

    MatchPresentationPhase Phase { get; }

    string HomeTeam { get; }

    string AwayTeam { get; }

    string Stadium { get; }

    string City { get; }

    string RoundLabel { get; }

    IReadOnlyList<string> HomeLineup { get; }

    IReadOnlyList<string> AwayLineup { get; }

    int Attendance { get; }

    bool Neutral { get; }

    int Minute { get; }

    int HomeScore { get; }

    int AwayScore { get; }

    string Commentary { get; }

    bool CommentaryIsColorized { get; }

    int CommentaryForeground { get; }

    int CommentaryBackground { get; }

    IReadOnlyList<MatchGoalVm> Goals { get; }

    IReadOnlyList<bool?> HomePenalties { get; }

    IReadOnlyList<bool?> AwayPenalties { get; }

    string ContinueText { get; }

    bool CanContinue { get; }

    Task ShowIntroductionAsync(
        GameState state,
        Fixture fixture,
        int stadiumTeamSlot,
        int attendance,
        bool neutral,
        CancellationToken ct);

    Task ShowClockAsync(
        GameState state,
        Fixture fixture,
        int minute,
        int homeScore,
        int awayScore,
        CancellationToken ct);

    Task PlayCommentaryAsync(
        IReadOnlyList<CommentaryPart> parts,
        CancellationToken ct);

    void ShowGoal(
        GameState state,
        Fixture fixture,
        int scoringSide,
        int scorerNumber,
        int minute,
        int homeScore,
        int awayScore);

    void StartPenaltyShootout();

    void ShowPenaltyMark(int scoringSide, int kickIndex, bool scored);

    Task ShowFinishedAsync(string result, bool penaltiesFollow, CancellationToken ct);

    void Hide();

    void Continue();
}
