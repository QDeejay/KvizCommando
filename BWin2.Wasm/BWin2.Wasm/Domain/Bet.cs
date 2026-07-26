namespace BWin2.Wasm.Domain;

internal enum BetEventKind
{
    Champion,
    CupWinner,
    LeagueMatch,
    CupMatch
}

internal enum BetSelection
{
    None,
    Home,
    Draw,
    Away
}

internal enum BetStatus
{
    Opened,
    Closed
}

internal sealed class Bet
{
    public required BetEventKind EventKind { get; init; }

    public required string EventName { get; init; }

    public required string WeekLabel { get; init; }

    public int EventWeek { get; init; }

    public int CupRound { get; init; }

    public int MatchNumber { get; init; }

    public BetSelection Selection { get; init; }

    public required string Tip { get; init; }

    public int Stake { get; init; }

    public required string OddsText { get; init; }

    public int PotentialWin { get; init; }

    public BetStatus Status { get; set; } = BetStatus.Opened;

    public bool Lost { get; set; }

    public string StatusText => Status == BetStatus.Opened ? "Opened" : "Closed";

    public string WinText => Lost
        ? "     Lost"
        : Qb.Right("          " + Qb.Str(PotentialWin), 9);
}
