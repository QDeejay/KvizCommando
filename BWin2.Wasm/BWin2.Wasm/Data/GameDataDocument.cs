namespace BWin2.Wasm.Data;

internal sealed class GameDataDocument
{
    public required List<TeamDocument> Teams { get; init; }

    public required List<string> LeagueRoundScripts { get; init; }

    public required CommentaryDocument Commentary { get; init; }
}

internal sealed class TeamDocument
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public required List<PlayerDocument> Players { get; init; }

    public required StadiumDocument Stadium { get; init; }

    public required string ShortName { get; init; }

    public required string Coach { get; init; }

    public int Strength { get; init; }
}

internal sealed class PlayerDocument
{
    public required string Name { get; init; }

    public required string Position { get; init; }
}

internal sealed class StadiumDocument
{
    public required string Name { get; init; }

    public required string City { get; init; }

    public int Capacity { get; init; }

    public int ForegroundColor { get; init; }

    public int BackgroundColor { get; init; }
}

internal sealed class CommentaryDocument
{
    public required List<string> Phrases { get; init; }

    public required List<string> Scripts { get; init; }
}
