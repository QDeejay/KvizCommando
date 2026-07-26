using BWin2.Wasm.Configuration;
using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Services;

internal sealed class BettingService : IBettingService
{
    public bool IsStakeValid(int stake, int credit) =>
        stake >= GameRules.MinimumStake &&
        stake <= GameRules.MaximumStake &&
        stake <= credit;

    public Bet PlaceSeasonBet(
        GameState state,
        BetEventKind kind,
        int selectedTeamSlot,
        string oddsText,
        int stake)
    {
        Team team = state.TeamAt(selectedTeamSlot);
        var bet = new Bet
        {
            EventKind = kind,
            EventName = kind == BetEventKind.Champion ? "Champion     " : "DFB Cup win  ",
            WeekLabel = "--  ",
            EventWeek = 0,
            Tip = team.ShortName,
            Stake = stake,
            OddsText = oddsText,
            PotentialWin = Qb.Int(Qb.Val(oddsText) * stake)
        };

        state.Credit -= stake;
        state.Bets.Add(bet);
        return bet;
    }

    public bool HasOpenMatchBet(
        GameState state,
        bool cup,
        int round,
        int matchNumber)
    {
        return state.Bets.Any(bet =>
            bet.EventKind == (cup ? BetEventKind.CupMatch : BetEventKind.LeagueMatch) &&
            (cup ? bet.CupRound == round : bet.EventWeek == round) &&
            bet.MatchNumber == matchNumber &&
            (!cup || bet.Status == BetStatus.Opened));
    }

    public Bet PlaceMatchBet(
        GameState state,
        Fixture fixture,
        bool cup,
        int round,
        BetSelection selection,
        MatchOdds odds,
        int stake)
    {
        Team homeTeam = state.TeamAt(fixture.HomeTeamSlot);
        Team awayTeam = state.TeamAt(fixture.AwayTeamSlot);
        string oddsText = odds.GetText(selection);
        var bet = new Bet
        {
            EventKind = cup ? BetEventKind.CupMatch : BetEventKind.LeagueMatch,
            EventName = homeTeam.ShortName + " - " + awayTeam.ShortName + "   ",
            WeekLabel = cup ? GameRules.CupEventCodes[round] : $" {round:00} ",
            EventWeek = cup ? GameRules.CupWeeks[round] : round,
            CupRound = cup ? round : 0,
            MatchNumber = fixture.MatchNumber,
            Selection = selection,
            Tip = SelectionText(selection),
            Stake = stake,
            OddsText = oddsText,
            PotentialWin = Qb.Int(Qb.Val(oddsText) * stake)
        };

        state.Credit -= stake;
        state.Bets.Add(bet);
        return bet;
    }

    public void SettleMatch(
        GameState state,
        bool cup,
        int round,
        int matchNumber,
        BetSelection result)
    {
        foreach (Bet bet in state.Bets.Where(bet =>
            bet.EventKind == (cup ? BetEventKind.CupMatch : BetEventKind.LeagueMatch) &&
            (cup ? bet.CupRound == round : bet.EventWeek == round) &&
            bet.MatchNumber == matchNumber))
        {
            bet.Status = BetStatus.Closed;
            if (bet.Selection == result)
                state.PendingCredit += bet.PotentialWin;
            else
                bet.Lost = true;
        }
    }

    public void ResolveSeasonBets(GameState state, IReadOnlyList<Team> leagueTable)
    {
        Bet? championBet = state.Bets.FirstOrDefault(
            bet => bet.EventKind == BetEventKind.Champion);

        if (state.CurrentCupRound == 5 && state.Week == 35 && championBet is not null)
        {
            championBet.Status = BetStatus.Closed;
            if (state.TeamAt(state.ChampionBetTeamSlot).Name == leagueTable[0].Name)
                state.PendingCredit += championBet.PotentialWin;
            else
                championBet.Lost = true;
        }

        Bet? cupWinnerBet = state.Bets.FirstOrDefault(
            bet => bet.EventKind == BetEventKind.CupWinner);

        if (state.CupRoundScripts[6] != string.Empty &&
            state.CurrentCupRound == 0 &&
            state.Week == 35 &&
            cupWinnerBet is not null)
        {
            cupWinnerBet.Status = BetStatus.Closed;
            int winnerSlot = state.CupRoundScripts[6][0] - 64;
            if (state.TeamAt(state.CupWinnerBetTeamSlot).Name == state.TeamAt(winnerSlot).Name)
                state.PendingCredit += cupWinnerBet.PotentialWin;
            else
                cupWinnerBet.Lost = true;
        }
    }

    public void RemoveOldMatchBets(GameState state)
    {
        state.Bets.RemoveAll(bet =>
            bet.EventKind is BetEventKind.LeagueMatch or BetEventKind.CupMatch &&
            bet.EventWeek < state.Week - 5 &&
            state.Week > 5);

        state.Bets.Sort((first, second) => first.EventWeek.CompareTo(second.EventWeek));
    }

    public bool IsMatchVisibleBecauseOfBet(
        GameState state,
        bool cup,
        int round,
        int matchNumber) =>
        state.Bets.Any(bet =>
            bet.EventKind == (cup ? BetEventKind.CupMatch : BetEventKind.LeagueMatch) &&
            (cup ? bet.CupRound == round : bet.EventWeek == round) &&
            bet.MatchNumber == matchNumber &&
            bet.Status == BetStatus.Opened);

    public static string SelectionText(BetSelection selection) => selection switch
    {
        BetSelection.Home => "Home",
        BetSelection.Draw => "Draw",
        BetSelection.Away => "Away",
        _ => string.Empty
    };
}
