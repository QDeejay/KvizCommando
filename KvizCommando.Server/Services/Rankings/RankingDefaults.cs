namespace KvizCommando.Server.Services.Rankings;

internal static class RankingDefaults
{
    // A mintasoroknak nincs játékos-azonosítójuk vagy adatbázisrekordjuk.
    internal static readonly RankingValue[] Players =
    [
        new(0, 10, 9999.9, "Alpha"),
        new(0, 9, 9999.9, "Bravo"),
        new(0, 8, 9999.9, "Charlie"),
        new(0, 7, 9999.9, "Delta"),
        new(0, 6, 9999.9, "Echo"),
        new(0, 5, 9999.9, "Foxtrot"),
        new(0, 4, 9999.9, "Golf"),
        new(0, 3, 9999.9, "Hotel"),
        new(0, 2, 9999.9, "India"),
        new(0, 1, 9999.9, "Juliett")
    ];
}
