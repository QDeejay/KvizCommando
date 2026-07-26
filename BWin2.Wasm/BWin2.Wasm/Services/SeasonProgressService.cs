using BWin2.Wasm.Configuration;
using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Services;

internal sealed class SeasonProgressService : ISeasonProgressService
{
    private readonly ILeagueService _leagueService;
    private readonly IBettingService _bettingService;

    public SeasonProgressService(
        ILeagueService leagueService,
        IBettingService bettingService)
    {
        _leagueService = leagueService;
        _bettingService = bettingService;
    }

    public void AdvanceAfterRound(GameState state)
    {
        bool cupRoundJustPlayed = state.CurrentCupRound != 0;

        if (!cupRoundJustPlayed && state.Week == 18)
            _leagueService.ApplyMidseasonAdjustments(state);

        if (cupRoundJustPlayed)
        {
            state.NextCupRound = Math.Min(
                state.NextCupRound + 1,
                GameRules.CupRoundCount);
        }
        else
        {
            state.Week++;
        }

        if (state.Week - 1 is 6 or 14 or 21 or 26 or 34)
            state.CurrentCupRound = state.NextCupRound;

        if (cupRoundJustPlayed)
            state.CurrentCupRound = 0;

        IReadOnlyList<Team> table = _leagueService.GetTable(state);
        _bettingService.ResolveSeasonBets(state, table);
        _leagueService.BuildScorerTable(state);
    }
}
