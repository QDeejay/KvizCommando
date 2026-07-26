using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Services;

internal interface IScheduleService
{
    void InitializeFormAdjustments(GameState state);

    void GenerateSeason(GameState state);

    IReadOnlyList<Fixture> GetLeagueFixtures(GameState state, int week);

    IReadOnlyList<Fixture> GetCupFixtures(GameState state, int cupRound);

    IReadOnlyList<Fixture> GetCurrentFixtures(GameState state);

    void AddCupWinner(GameState state, int teamSlot);
}
