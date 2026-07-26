using System.Net.Http.Json;
using System.Text.Json;
using BWin2.Wasm.Configuration;
using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Data;

internal sealed class WasmGameDataStore : IGameDataStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public WasmGameDataStore(HttpClient http)
    {
        _http = http;
    }

    public async Task<GameState> LoadAsync(CancellationToken ct = default)
    {
        GameDataDocument document =
            await _http.GetFromJsonAsync<GameDataDocument>(
                "data/game-data.json",
                JsonOptions,
                ct) ??
            throw new InvalidDataException("The BWIN2 game data file is empty.");

        Validate(document);

        List<Team> teams = document.Teams
            .Select(team => new Team
            {
                Id = team.Id,
                Name = team.Name,
                ShortName = team.ShortName,
                Coach = team.Coach,
                Strength = team.Strength,
                Stadium = new Stadium
                {
                    Name = team.Stadium.Name,
                    City = team.Stadium.City,
                    Capacity = team.Stadium.Capacity,
                    ForegroundColor = team.Stadium.ForegroundColor,
                    BackgroundColor = team.Stadium.BackgroundColor
                },
                Players = team.Players
                    .Select(player => new Player
                    {
                        Name = player.Name,
                        Position = player.Position
                    })
                    .ToList()
            })
            .ToList();

        string[] leagueRounds = new string[GameRules.LeagueRoundCount + 1];
        for (int round = 1; round <= GameRules.LeagueRoundCount; round++)
            leagueRounds[round] = document.LeagueRoundScripts[round - 1];

        return new GameState(
            teams,
            leagueRounds,
            new CommentaryCatalog(
                document.Commentary.Phrases,
                document.Commentary.Scripts));
    }

    private static void Validate(GameDataDocument document)
    {
        if (document.Teams.Count != GameRules.AllTeamCount)
            throw new InvalidDataException($"Expected {GameRules.AllTeamCount} teams.");

        if (document.Teams.Any(team =>
                team.Id < 1 ||
                team.Id > GameRules.AllTeamCount ||
                team.Players.Count != GameRules.PlayerCountPerTeam))
        {
            throw new InvalidDataException(
                $"Every team must have a valid id and {GameRules.PlayerCountPerTeam} players.");
        }

        if (document.Teams.Select(team => team.Id).Distinct().Count() !=
            GameRules.AllTeamCount)
        {
            throw new InvalidDataException("Team ids must be unique.");
        }

        if (document.LeagueRoundScripts.Count != GameRules.LeagueRoundCount ||
            document.LeagueRoundScripts.Any(script => script.Length != 18))
        {
            throw new InvalidDataException(
                "The 34 league scripts must contain nine fixtures.");
        }

        if (document.Commentary.Phrases.Count != 24 ||
            document.Commentary.Scripts.Count != 75)
        {
            throw new InvalidDataException(
                "The commentary dictionary must contain 24 phrases and 75 scripts.");
        }
    }
}
