using BWin2.Wasm.Configuration;
using BWin2.Wasm.Domain;

namespace BWin2.Wasm.State;

internal sealed class MatchPresentation : IMatchPresentation
{
    private readonly List<MatchGoalVm> _goals = [];
    private readonly bool?[] _homePenalties = new bool?[11];
    private readonly bool?[] _awayPenalties = new bool?[11];
    private TaskCompletionSource? _continueSource;

    public event Action? Changed;

    public bool IsActive { get; private set; }

    public MatchPresentationPhase Phase { get; private set; } =
        MatchPresentationPhase.Hidden;

    public string HomeTeam { get; private set; } = string.Empty;

    public string AwayTeam { get; private set; } = string.Empty;

    public string Stadium { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string RoundLabel { get; private set; } = string.Empty;

    public IReadOnlyList<string> HomeLineup { get; private set; } = [];

    public IReadOnlyList<string> AwayLineup { get; private set; } = [];

    public int Attendance { get; private set; }

    public bool Neutral { get; private set; }

    public int Minute { get; private set; }

    public int HomeScore { get; private set; }

    public int AwayScore { get; private set; }

    public string Commentary { get; private set; } = string.Empty;

    public bool CommentaryIsColorized { get; private set; }

    public int CommentaryForeground { get; private set; } = 3;

    public int CommentaryBackground { get; private set; } = 1;

    public IReadOnlyList<MatchGoalVm> Goals => _goals;

    public IReadOnlyList<bool?> HomePenalties => _homePenalties;

    public IReadOnlyList<bool?> AwayPenalties => _awayPenalties;

    public string ContinueText { get; private set; } = string.Empty;

    public bool CanContinue => _continueSource is not null;

    public async Task ShowIntroductionAsync(
        GameState state,
        Fixture fixture,
        int stadiumTeamSlot,
        int attendance,
        bool neutral,
        CancellationToken ct)
    {
        Team stadiumTeam = state.TeamAt(stadiumTeamSlot);
        IsActive = true;
        Phase = MatchPresentationPhase.Introduction;
        HomeTeam = state.TeamAt(fixture.HomeTeamSlot).Name;
        AwayTeam = state.TeamAt(fixture.AwayTeamSlot).Name;
        Stadium = stadiumTeam.Stadium.Name;
        City = stadiumTeam.Stadium.City;
        RoundLabel = state.CurrentCupRound == 0
            ? $"Championship · {state.Week}. hét"
            : $"DFB Cup · {GameRules.CupRoundNames[state.CurrentCupRound].Trim()}";
        HomeLineup = state.TeamAt(fixture.HomeTeamSlot)
            .Players.Select((player, index) => $"{index + 1}. {player.Name}")
            .ToArray();
        AwayLineup = state.TeamAt(fixture.AwayTeamSlot)
            .Players.Select((player, index) => $"{index + 1}. {player.Name}")
            .ToArray();
        Attendance = attendance;
        Neutral = neutral;
        Minute = 0;
        HomeScore = 0;
        AwayScore = 0;
        Commentary = string.Empty;
        CommentaryIsColorized = false;
        _goals.Clear();
        Array.Fill(_homePenalties, null);
        Array.Fill(_awayPenalties, null);
        ContinueText = "Mérkőzés indítása";
        NotifyChanged();
        await WaitForContinueAsync(ct);
    }

    public async Task ShowClockAsync(
        GameState state,
        Fixture fixture,
        int minute,
        int homeScore,
        int awayScore,
        CancellationToken ct)
    {
        Phase = MatchPresentationPhase.Live;
        Minute = minute;
        HomeScore = homeScore;
        AwayScore = awayScore;
        ContinueText = string.Empty;
        NotifyChanged();
        await Task.Delay(GameRules.MatchMinuteMilliseconds, ct);
    }

    public async Task PlayCommentaryAsync(
        IReadOnlyList<CommentaryPart> parts,
        CancellationToken ct)
    {
        foreach (CommentaryPart part in parts)
        {
            Commentary = part.Text.Trim();
            CommentaryIsColorized = part.Colorize;
            CommentaryForeground = part.ForegroundColor;
            CommentaryBackground = part.BackgroundColor;
            NotifyChanged();
            await Task.Delay(GameRules.CommentaryMilliseconds, ct);
        }
    }

    public void ShowGoal(
        GameState state,
        Fixture fixture,
        int scoringSide,
        int scorerNumber,
        int minute,
        int homeScore,
        int awayScore)
    {
        Team team = state.TeamAt(
            scoringSide == 0 ? fixture.HomeTeamSlot : fixture.AwayTeamSlot);
        _goals.Add(new MatchGoalVm(
            team.Players[scorerNumber - 1].Name,
            team.ShortName.Trim(),
            minute,
            homeScore,
            awayScore));
        HomeScore = homeScore;
        AwayScore = awayScore;
        NotifyChanged();
    }

    public void StartPenaltyShootout()
    {
        Phase = MatchPresentationPhase.Penalties;
        Commentary = "Draw. Penalty kicks decide today.";
        CommentaryIsColorized = false;
        ContinueText = string.Empty;
        NotifyChanged();
    }

    public void ShowPenaltyMark(int scoringSide, int kickIndex, bool scored)
    {
        int index = Math.Clamp(kickIndex - 1, 0, 10);
        if (scoringSide == 0)
            _homePenalties[index] = scored;
        else
            _awayPenalties[index] = scored;
        NotifyChanged();
    }

    public async Task ShowFinishedAsync(
        string result,
        bool penaltiesFollow,
        CancellationToken ct)
    {
        Phase = penaltiesFollow
            ? MatchPresentationPhase.Penalties
            : MatchPresentationPhase.Finished;
        Commentary = penaltiesFollow
            ? $"Hosszabbítás után {result}. Büntetőrúgások következnek."
            : $"Végeredmény: {result}";
        CommentaryIsColorized = false;
        ContinueText = penaltiesFollow ? "Büntetőrúgások" : "Tovább";
        NotifyChanged();
        await WaitForContinueAsync(ct);
    }

    public void Hide()
    {
        IsActive = false;
        Phase = MatchPresentationPhase.Hidden;
        _continueSource?.TrySetCanceled();
        _continueSource = null;
        NotifyChanged();
    }

    public void Continue()
    {
        _continueSource?.TrySetResult();
    }

    private async Task WaitForContinueAsync(CancellationToken ct)
    {
        _continueSource = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        NotifyChanged();

        using CancellationTokenRegistration registration =
            ct.Register(() => _continueSource.TrySetCanceled(ct));
        await _continueSource.Task;
        _continueSource = null;
        NotifyChanged();
    }

    private void NotifyChanged() => Changed?.Invoke();
}
