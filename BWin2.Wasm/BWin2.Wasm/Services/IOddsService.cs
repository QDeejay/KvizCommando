using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Services;

internal interface IOddsService
{
    MatchOdds CalculateMatchOdds(
        Team homeTeam,
        Team awayTeam,
        bool cup,
        int cupRound);

    string CalculatePreseasonOdds(
        Team referenceTeam,
        Team selectedTeam,
        bool cupWinner);
}
