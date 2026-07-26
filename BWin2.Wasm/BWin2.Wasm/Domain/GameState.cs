using BWin2.Wasm.Configuration;

namespace BWin2.Wasm.Domain;

internal sealed class GameState
{
    private readonly Team?[] _teams = new Team?[GameRules.AllTeamCount + 1];

    public GameState(
        IReadOnlyList<Team> teams,
        string[] leagueRoundScripts,
        CommentaryCatalog commentary)
    {
        foreach (Team team in teams)
            _teams[team.Id] = team;

        LeagueRoundScripts = leagueRoundScripts;
        Commentary = commentary;
        ClearSeasonResults();
    }

    public string[] LeagueRoundScripts { get; }

    public string[] CupRoundScripts { get; } = new string[GameRules.CupRoundCount + 2];

    public string[,] LeagueResults { get; } =
        new string[GameRules.LeagueRoundCount + 1, 10];

    public string[,] CupResults { get; } =
        new string[GameRules.CupRoundCount + 1, 17];

    public int[] LeagueTeamOrder { get; } = new int[GameRules.AllTeamCount + 1];

    public CommentaryCatalog Commentary { get; }

    public List<Bet> Bets { get; } = [];

    public List<ScorerEntry> Scorers { get; } = [];

    public SeasonMarks Marks { get; } = new();

    public int Credit { get; set; } = GameRules.StartingCredit;

    public int PendingCredit { get; set; }

    public int Week { get; set; } = 1;

    public int CurrentCupRound { get; set; }

    public int NextCupRound { get; set; } = 1;

    public int ChampionBetTeamSlot { get; set; } = 1;

    public int CupWinnerBetTeamSlot { get; set; } = 1;

    public bool HasStarted { get; set; }

    public Team TeamAt(int slot) =>
        _teams[slot] ?? throw new InvalidDataException($"Missing team slot {slot}.");

    public void SwapTeamSlots(int first, int second)
    {
        (_teams[first], _teams[second]) = (_teams[second], _teams[first]);
        TeamAt(first).Id = first;
        TeamAt(second).Id = second;
    }

    public IEnumerable<Team> FirstDivisionTeams()
    {
        for (int slot = 1; slot <= GameRules.FirstDivisionTeamCount; slot++)
            yield return TeamAt(slot);
    }

    public string GetResult(bool cup, int round, int matchNumber) =>
        cup
            ? CupResults[round, matchNumber] ?? string.Empty
            : LeagueResults[round, matchNumber] ?? string.Empty;

    public void SetResult(bool cup, int round, int matchNumber, string value)
    {
        if (cup)
            CupResults[round, matchNumber] = value;
        else
            LeagueResults[round, matchNumber] = value;
    }

    public void ClearSeasonResults()
    {
        Array.Clear(LeagueResults, 0, LeagueResults.Length);
        Array.Clear(CupResults, 0, CupResults.Length);
        Array.Fill(CupRoundScripts, string.Empty);
        Scorers.Clear();
    }
}
