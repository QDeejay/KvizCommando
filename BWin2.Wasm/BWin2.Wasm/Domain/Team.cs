namespace BWin2.Wasm.Domain;

internal sealed class Team
{
    public int Id { get; set; }

    public required string Name { get; init; }

    public required List<Player> Players { get; init; }

    public required Stadium Stadium { get; init; }

    public required string ShortName { get; init; }

    public required string Coach { get; init; }

    public int Strength { get; set; }

    public int SeasonAdjustment { get; set; }

    public TeamStatistics Statistics { get; } = new();
}
