using BWin2.Wasm.Configuration;
using BWin2.Wasm.Domain;
using BWin2.Wasm.State;

namespace BWin2.Wasm.Services;

internal sealed class MatchEngine : IMatchEngine
{
    private readonly IRandomSource _random;
    private readonly IScheduleService _scheduleService;
    private readonly IBettingService _bettingService;
    private readonly ICommentaryScriptService _commentary;
    private readonly IMatchPresentation _presentation;

    public MatchEngine(
        IRandomSource random,
        IScheduleService scheduleService,
        IBettingService bettingService,
        ICommentaryScriptService commentary,
        IMatchPresentation presentation)
    {
        _random = random;
        _scheduleService = scheduleService;
        _bettingService = bettingService;
        _commentary = commentary;
        _presentation = presentation;
    }

    public async Task<RoundPlayResult> PlayRoundAsync(
        GameState state,
        bool showBetMatches,
        CancellationToken ct = default)
    {
        bool cup = state.CurrentCupRound != 0;
        int round = cup ? state.CurrentCupRound : state.Week;
        IReadOnlyList<Fixture> fixtures = cup
            ? _scheduleService.GetCupFixtures(state, round)
            : _scheduleService.GetLeagueFixtures(state, round);
        var playedMatches = new List<PlayedMatch>(fixtures.Count);
        bool hadVisibleMatch = false;

        try
        {
            foreach (Fixture fixture in fixtures)
            {
                ct.ThrowIfCancellationRequested();
                bool visible =
                    showBetMatches &&
                    _bettingService.IsMatchVisibleBecauseOfBet(
                        state,
                        cup,
                        round,
                        fixture.MatchNumber);

                if (visible)
                {
                    hadVisibleMatch = true;
                    await ShowIntroductionAsync(state, fixture, ct);
                }

                PlayedMatch match = await SimulateAsync(
                    state,
                    fixture,
                    visible,
                    ct);
                playedMatches.Add(match);
                StoreResult(state, match, cup, round);

                if (visible)
                    _presentation.Hide();
            }
        }
        finally
        {
            _presentation.Hide();
        }

        return new RoundPlayResult(playedMatches, hadVisibleMatch);
    }

    private async Task ShowIntroductionAsync(
        GameState state,
        Fixture fixture,
        CancellationToken ct)
    {
        int stadiumTeamSlot = fixture.HomeTeamSlot;

        if (state.CurrentCupRound == 4)
        {
            do
            {
                stadiumTeamSlot =
                    Qb.Int(_random.Next() * GameRules.FirstDivisionTeamCount) + 1;
            }
            while (stadiumTeamSlot == fixture.HomeTeamSlot ||
                   stadiumTeamSlot == fixture.AwayTeamSlot);
        }

        if (state.CurrentCupRound == 5)
        {
            stadiumTeamSlot = 1;
            while (state.TeamAt(stadiumTeamSlot).Stadium.City != "Berlin")
                stadiumTeamSlot++;
        }

        Team stadiumTeam = state.TeamAt(stadiumTeamSlot);
        int attendance =
            Qb.Int(stadiumTeam.Stadium.Capacity * .9) +
            Qb.Int(
                _random.Next() *
                Qb.Int(stadiumTeam.Stadium.Capacity * .1));

        if (state.CurrentCupRound == 5)
            attendance = stadiumTeam.Stadium.Capacity;

        bool neutral =
            state.CurrentCupRound > 3 &&
            stadiumTeamSlot != fixture.HomeTeamSlot &&
            stadiumTeamSlot != fixture.AwayTeamSlot;
        await _presentation.ShowIntroductionAsync(
            state,
            fixture,
            stadiumTeamSlot,
            attendance,
            neutral,
            ct);
    }

    private async Task<PlayedMatch> SimulateAsync(
        GameState state,
        Fixture fixture,
        bool visible,
        CancellationToken ct)
    {
        Team homeTeam = state.TeamAt(fixture.HomeTeamSlot);
        Team awayTeam = state.TeamAt(fixture.AwayTeamSlot);
        double homeStrength = homeTeam.Strength + homeTeam.SeasonAdjustment;
        double awayStrength = awayTeam.Strength + awayTeam.SeasonAdjustment;
        int randomBalance = Qb.Int(_random.Next() * 12) + 1;

        if (randomBalance == 10)
            homeStrength = awayStrength;

        double homeChance =
            (state.CurrentCupRound >= 4 ? 50 : 55) +
            homeStrength -
            awayStrength;
        double strengthGap = Math.Min(Math.Abs(homeStrength - awayStrength), 20);
        _ = Qb.Int(_random.Next() * (22 - Qb.Int(strengthGap / 2)));

        int minute = 1;
        int homeScore = 0;
        int awayScore = 0;
        int extraTime = 0;

        do
        {
            if (visible)
            {
                await _presentation.ShowClockAsync(
                    state,
                    fixture,
                    minute,
                    homeScore,
                    awayScore,
                    ct);
            }

            homeChance =
                (state.CurrentCupRound >= 4 ? 50 : 55) +
                homeStrength -
                awayStrength;
            homeChance = Math.Clamp(homeChance, 25, 85);
            strengthGap = Math.Min(Math.Abs(homeStrength - awayStrength), 20);

            int eventCode =
                Qb.Int(_random.Next() * (40 - Qb.Int(strengthGap / 2)));
            int eventSideRoll = Qb.Int(_random.Next() * 100) + 1;

            if (visible && eventCode is 3 or 4)
            {
                int commentCode = Qb.Int(_random.Next() * 54) + 21;
                int side = eventSideRoll <= homeChance ? 0 : 1;
                int playerNumber = DrawScoringPlayer();
                await _presentation.PlayCommentaryAsync(
                    _commentary.BuildMatchCommentary(
                        state,
                        fixture,
                        side,
                        playerNumber,
                        commentCode),
                    ct);
            }

            if (eventCode == 2)
            {
                int goalSideRoll = Qb.Int(_random.Next() * 100) + 1;
                int commentCode = Qb.Int(_random.Next() * 20) + 1;
                int side = goalSideRoll <= homeChance ? 0 : 1;
                int playerNumber = DrawScoringPlayer();

                if (side == 0)
                    homeScore++;
                else
                    awayScore++;

                if (visible)
                {
                    await _presentation.PlayCommentaryAsync(
                        _commentary.BuildMatchCommentary(
                            state,
                            fixture,
                            side,
                            playerNumber,
                            commentCode),
                        ct);
                    _presentation.ShowGoal(
                        state,
                        fixture,
                        side,
                        playerNumber,
                        minute,
                        homeScore,
                        awayScore);
                }

                if (state.CurrentCupRound == 0)
                {
                    Team scoringTeam = side == 0 ? homeTeam : awayTeam;
                    scoringTeam.Players[playerNumber - 1].Goals++;
                }
            }

            minute++;
            if (state.CurrentCupRound != 0 &&
                homeScore == awayScore &&
                minute == 91)
            {
                extraTime = 30;
            }
        }
        while (minute != 91 + extraTime);

        bool penalties = false;
        int penaltyHomeScore = 0;
        int penaltyAwayScore = 0;
        if (state.CurrentCupRound != 0 &&
            homeScore == awayScore &&
            extraTime == 30)
        {
            penalties = true;
            if (visible)
            {
                await _presentation.ShowFinishedAsync(
                    BuildResult(homeScore, awayScore, true, false, 0, 0),
                    penaltiesFollow: true,
                    ct);
            }

            (penaltyHomeScore, penaltyAwayScore) =
                await PlayPenaltyShootoutAsync(state, fixture, visible, ct);
        }

        string result = BuildResult(
            homeScore,
            awayScore,
            extraTime == 30,
            penalties,
            penaltyHomeScore,
            penaltyAwayScore);

        if (visible)
        {
            await _presentation.ShowFinishedAsync(
                result,
                penaltiesFollow: false,
                ct);
        }

        return new PlayedMatch(
            fixture,
            homeScore,
            awayScore,
            extraTime == 30,
            penalties,
            penaltyHomeScore,
            penaltyAwayScore,
            result);
    }

    private int DrawScoringPlayer()
    {
        int roll = Qb.Int(_random.Next() * 100) + 1;
        if (roll > 51)
            return Qb.Int(_random.Next() * 2) + 10;
        if (roll > 16)
            return Qb.Int(_random.Next() * 4) + 6;
        return Qb.Int(_random.Next() * 4) + 2;
    }

    private async Task<(int Home, int Away)> PlayPenaltyShootoutAsync(
        GameState state,
        Fixture fixture,
        bool visible,
        CancellationToken ct)
    {
        int homeScore = 0;
        int awayScore = 0;
        int kickIndex = visible ? 1 : 0;
        int kicksTaken = 0;

        if (visible)
        {
            _presentation.StartPenaltyShootout();
            await _presentation.PlayCommentaryAsync(
                _commentary.BuildPenaltyCommentary(
                    state,
                    fixture,
                    0,
                    kickIndex,
                    "@"),
                ct);
        }

        while (true)
        {
            int shot = Qb.Int(_random.Next() * 100);
            bool homeGoal = shot < 69;

            if (visible)
            {
                await _presentation.PlayCommentaryAsync(
                    _commentary.BuildPenaltyCommentary(
                        state,
                        fixture,
                        0,
                        kickIndex,
                        homeGoal ? "KLS" : "KLZ"),
                    ct);
            }

            if (homeGoal)
                homeScore++;
            if (visible)
                _presentation.ShowPenaltyMark(0, kickIndex, homeGoal);

            kicksTaken++;
            if (Math.Abs(homeScore - awayScore) > 2 ||
                ((kicksTaken - (kickIndex - 1)) > 4 &&
                 Math.Abs(homeScore - awayScore) == 2) ||
                (Math.Abs(homeScore - awayScore) == 1 &&
                 kicksTaken - (kickIndex - 1) == 5 &&
                 awayScore > homeScore))
            {
                return (homeScore, awayScore);
            }

            shot = Qb.Int(_random.Next() * 100);
            bool awayGoal = shot < 69;
            if (awayGoal)
                awayScore++;

            if (visible)
            {
                await _presentation.PlayCommentaryAsync(
                    _commentary.BuildPenaltyCommentary(
                        state,
                        fixture,
                        1,
                        kickIndex,
                        awayGoal ? "KLS" : "KLZ"),
                    ct);
                _presentation.ShowPenaltyMark(1, kickIndex, awayGoal);
            }

            kicksTaken++;
            if (Math.Abs(homeScore - awayScore) > 2 ||
                (kicksTaken / 2d > 3 &&
                 Math.Abs(homeScore - awayScore) == 2))
            {
                return (homeScore, awayScore);
            }

            kickIndex++;
            if (kickIndex > 11)
                kickIndex = 1;
            if (homeScore != awayScore && kicksTaken >= 10)
                return (homeScore, awayScore);
        }
    }

    private void StoreResult(
        GameState state,
        PlayedMatch match,
        bool cup,
        int round)
    {
        Fixture fixture = match.Fixture;
        Team homeTeam = state.TeamAt(fixture.HomeTeamSlot);
        Team awayTeam = state.TeamAt(fixture.AwayTeamSlot);
        BetSelection result;

        if (match.Penalties)
        {
            result = match.PenaltyHomeScore > match.PenaltyAwayScore
                ? BetSelection.Home
                : BetSelection.Away;
        }
        else if (match.HomeScore == match.AwayScore)
            result = BetSelection.Draw;
        else
        {
            result = match.HomeScore > match.AwayScore
                ? BetSelection.Home
                : BetSelection.Away;
        }

        if (!cup)
        {
            if (result == BetSelection.Draw)
            {
                homeTeam.Statistics.Draws++;
                awayTeam.Statistics.Draws++;
                homeTeam.Statistics.ResultHistory += "D";
                awayTeam.Statistics.ResultHistory += "D";
            }
            else if (result == BetSelection.Home)
            {
                homeTeam.Statistics.Wins++;
                awayTeam.Statistics.Losses++;
                homeTeam.Statistics.ResultHistory += "W";
                awayTeam.Statistics.ResultHistory += "L";
            }
            else
            {
                homeTeam.Statistics.Losses++;
                awayTeam.Statistics.Wins++;
                homeTeam.Statistics.ResultHistory += "L";
                awayTeam.Statistics.ResultHistory += "W";
            }

            homeTeam.Statistics.GoalsFor += match.HomeScore;
            homeTeam.Statistics.GoalsAgainst += match.AwayScore;
            awayTeam.Statistics.GoalsFor += match.AwayScore;
            awayTeam.Statistics.GoalsAgainst += match.HomeScore;
        }
        else
        {
            _scheduleService.AddCupWinner(
                state,
                result == BetSelection.Home
                    ? fixture.HomeTeamSlot
                    : fixture.AwayTeamSlot);
        }

        state.SetResult(cup, round, fixture.MatchNumber, match.ResultText);
        _bettingService.SettleMatch(
            state,
            cup,
            round,
            fixture.MatchNumber,
            result);
    }

    private static string BuildResult(
        int homeScore,
        int awayScore,
        bool extraTime,
        bool penalties,
        int penaltyHomeScore,
        int penaltyAwayScore)
    {
        string result = $"{homeScore}:{awayScore}";
        if (extraTime)
            result += " (AET)";
        if (penalties)
            result += $" P({penaltyHomeScore}-{penaltyAwayScore})";
        return result;
    }
}
