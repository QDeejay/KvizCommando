namespace BWin2.Wasm.Domain;

internal sealed class Stadium
{
    public required string Name { get; init; }

    public required string City { get; init; }

    public int Capacity { get; init; }

    public int ForegroundColor { get; init; }

    public int BackgroundColor { get; init; }
}
