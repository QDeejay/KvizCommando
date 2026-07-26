using BWin2.Wasm.Configuration;
using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Services;

internal sealed class LeagueService : ILeagueService
{
    private readonly IRandomSource _random;

    public LeagueService(IRandomSource random)
    {
        _random = random;
    }

    public IReadOnlyList<Team> GetTable(GameState state) =>
        state.FirstDivisionTeams()
            .OrderByDescending(team => team.Statistics.Points)
            .ThenByDescending(team => team.Statistics.GoalDifference)
            .ThenByDescending(team => team.Statistics.GoalsFor)
            .ToList();

    public void ApplyMidseasonAdjustments(GameState state)
    {
        for (int slot = 0; slot <= GameRules.FirstDivisionTeamCount; slot++)
        {
            _random.Randomize();
            int change = Qb.Int(_random.Next() * 11) - 5;
            if (slot > 0)
                state.TeamAt(slot).SeasonAdjustment += change;
        }
    }

    public void BuildScorerTable(GameState state)
    {
        state.Scorers.Clear();

        for (int slot = 1; slot <= GameRules.FirstDivisionTeamCount; slot++)
        {
            Team team = state.TeamAt(slot);
            for (int player = 1; player < team.Players.Count; player++)
            {
                Player scorer = team.Players[player];
                state.Scorers.Add(new ScorerEntry(
                    scorer.Name,
                    team.ShortName,
                    scorer.Goals,
                    slot == state.ChampionBetTeamSlot));
            }
        }

        List<ScorerEntry> ordered = state.Scorers
            .OrderByDescending(entry => entry.Goals)
            .ToList();
        state.Scorers.Clear();
        state.Scorers.AddRange(ordered);
    }

    public void PrepareNewSeason(GameState state)
    {
        IReadOnlyList<Team> oldTable = GetTable(state);
        state.Marks.PreviousCupWinner = state.TeamAt(state.CupRoundScripts[6][0] - 64).Name;

        var promotionCandidates = new List<(Team Team, int Performance)>();
        for (int slot = 19; slot <= GameRules.AllTeamCount; slot++)
        {
            Team team = state.TeamAt(slot);
            int performance = team.Strength + Qb.Int(_random.Next() * 30);
            promotionCandidates.Add((team, performance));
        }

        promotionCandidates = promotionCandidates
            .OrderByDescending(item => item.Performance)
            .ToList();

        for (int slot = 19; slot <= GameRules.AllTeamCount; slot++)
        {
            Team desiredTeam = promotionCandidates[slot - 19].Team;
            int currentSlot = FindTeamSlot(state, desiredTeam);
            if (currentSlot != slot)
                state.SwapTeamSlots(slot, currentSlot);
        }

        foreach (Team team in state.FirstDivisionTeams())
        {
            team.Statistics.Reset();
            foreach (Player player in team.Players)
                player.Goals = 0;

            team.Strength = Math.Clamp(
                team.Strength + team.SeasonAdjustment,
                50,
                85);
            team.SeasonAdjustment = Qb.Int(_random.Next() * 21) - 10;
        }

        for (int relegatedIndex = 0; relegatedIndex < 3; relegatedIndex++)
        {
            Team relegated = oldTable[15 + relegatedIndex];
            int relegatedSlot = FindTeamSlot(state, relegated);
            int promotedSlot = 19 + relegatedIndex;
            state.SwapTeamSlots(relegatedSlot, promotedSlot);
            state.TeamAt(relegatedSlot).Strength = 60;
            state.TeamAt(relegatedSlot).SeasonAdjustment = 0;
        }

        SortTeamSlotsByStrength(state, 1, 18);
        SortTeamSlotsByStrength(state, 19, 32);

        state.Bets.Clear();
        state.ClearSeasonResults();
        state.Marks.Champion = oldTable[0].Name;
        state.Marks.Relegated16 = oldTable[15].Name;
        state.Marks.Relegated17 = oldTable[16].Name;
        state.Marks.Relegated18 = oldTable[17].Name;
        state.Week = 1;
        state.CurrentCupRound = 0;
        state.NextCupRound = 1;
    }

    private static int FindTeamSlot(GameState state, Team target)
    {
        for (int slot = 1; slot <= GameRules.AllTeamCount; slot++)
        {
            if (ReferenceEquals(state.TeamAt(slot), target))
                return slot;
        }

        throw new InvalidOperationException("Team is not present in the current season.");
    }

    private static void SortTeamSlotsByStrength(GameState state, int first, int last)
    {
        for (int left = first; left <= last; left++)
        for (int right = first; right <= last; right++)
        {
            if (state.TeamAt(left).Strength > state.TeamAt(right).Strength)
                state.SwapTeamSlots(left, right);
        }
    }
}
