using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Services;

internal interface ILeagueService
{
    IReadOnlyList<Team> GetTable(GameState state);

    void ApplyMidseasonAdjustments(GameState state);

    void BuildScorerTable(GameState state);

    void PrepareNewSeason(GameState state);
}
