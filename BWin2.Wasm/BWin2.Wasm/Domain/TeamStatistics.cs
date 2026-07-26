namespace BWin2.Wasm.Domain;

internal sealed class TeamStatistics
{
    public int Wins { get; set; }

    public int Draws { get; set; }

    public int Losses { get; set; }

    public int GoalsFor { get; set; }

    public int GoalsAgainst { get; set; }

    public string ResultHistory { get; set; } = string.Empty;

    public int Points => Wins * 3 + Draws;

    public int GoalDifference => GoalsFor - GoalsAgainst;

    public void Reset()
    {
        Wins = 0;
        Draws = 0;
        Losses = 0;
        GoalsFor = 0;
        GoalsAgainst = 0;
        ResultHistory = string.Empty;
    }
}
