using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Services;

internal interface IBettingService
{
    bool IsStakeValid(int stake, int credit);

    Bet PlaceSeasonBet(
        GameState state,
        BetEventKind kind,
        int selectedTeamSlot,
        string oddsText,
        int stake);

    bool HasOpenMatchBet(
        GameState state,
        bool cup,
        int round,
        int matchNumber);

    Bet PlaceMatchBet(
        GameState state,
        Fixture fixture,
        bool cup,
        int round,
        BetSelection selection,
        MatchOdds odds,
        int stake);

    void SettleMatch(
        GameState state,
        bool cup,
        int round,
        int matchNumber,
        BetSelection result);

    void ResolveSeasonBets(GameState state, IReadOnlyList<Team> leagueTable);

    void RemoveOldMatchBets(GameState state);

    bool IsMatchVisibleBecauseOfBet(
        GameState state,
        bool cup,
        int round,
        int matchNumber);
}
