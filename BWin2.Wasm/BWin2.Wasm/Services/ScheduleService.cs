using System.Text;
using BWin2.Wasm.Configuration;
using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Services;

internal sealed class ScheduleService : IScheduleService
{
    private readonly IRandomSource _random;

    public ScheduleService(IRandomSource random)
    {
        _random = random;
    }

    public void InitializeFormAdjustments(GameState state)
    {
        for (int slot = 1; slot <= GameRules.FirstDivisionTeamCount; slot++)
            state.TeamAt(slot).SeasonAdjustment = Qb.Int(_random.Next() * 21) - 10;
    }

    public void GenerateSeason(GameState state)
    {
        state.CupRoundScripts[1] = CreateRandomCupDraw();

        for (int slot = 1; slot <= GameRules.FirstDivisionTeamCount; slot++)
        {
            state.LeagueTeamOrder[slot] = state.HasStarted
                ? DrawUniqueTeam(state.LeagueTeamOrder, slot, GameRules.FirstDivisionTeamCount)
                : slot;
        }

        state.Week = 1;
        state.CurrentCupRound = 0;
        state.NextCupRound = 1;
    }

    public IReadOnlyList<Fixture> GetLeagueFixtures(GameState state, int week) =>
        DecodeFixtures(
            state.LeagueRoundScripts[week],
            slot => state.LeagueTeamOrder[slot]);

    public IReadOnlyList<Fixture> GetCupFixtures(GameState state, int cupRound) =>
        DecodeFixtures(state.CupRoundScripts[cupRound], slot => slot);

    public IReadOnlyList<Fixture> GetCurrentFixtures(GameState state) =>
        state.CurrentCupRound == 0
            ? GetLeagueFixtures(state, state.Week)
            : GetCupFixtures(state, state.CurrentCupRound);

    public void AddCupWinner(GameState state, int teamSlot)
    {
        int targetRound = state.CurrentCupRound + 1;
        state.CupRoundScripts[targetRound] += (char)(teamSlot + 64);
    }

    private string CreateRandomCupDraw()
    {
        int[] used = new int[GameRules.AllTeamCount + 1];
        var script = new StringBuilder(GameRules.AllTeamCount);

        for (int position = 1; position <= GameRules.AllTeamCount; position++)
        {
            int team = DrawUniqueTeam(used, position, GameRules.AllTeamCount);
            used[position] = team;
            script.Append((char)(team + 64));
        }

        return script.ToString();
    }

    private int DrawUniqueTeam(int[] used, int position, int teamCount)
    {
        int team;
        bool duplicate;

        do
        {
            duplicate = false;
            team = Qb.Int(_random.Next() * teamCount) + 1;

            for (int previous = 1; previous < position; previous++)
            {
                if (used[previous] == team)
                    duplicate = true;
            }
        }
        while (duplicate);

        return team;
    }

    private static IReadOnlyList<Fixture> DecodeFixtures(
        string script,
        Func<int, int> mapTeam)
    {
        var fixtures = new List<Fixture>(script.Length / 2);

        for (int offset = 0; offset < script.Length; offset += 2)
        {
            int home = mapTeam(script[offset] - 64);
            int away = mapTeam(script[offset + 1] - 64);
            fixtures.Add(new Fixture(offset / 2 + 1, home, away));
        }

        return fixtures;
    }
}
