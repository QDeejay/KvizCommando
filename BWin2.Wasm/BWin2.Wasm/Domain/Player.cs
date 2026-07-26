namespace BWin2.Wasm.Domain;

internal sealed class Player
{
    public required string Name { get; init; }

    public required string Position { get; init; }

    public int Goals { get; set; }
}
