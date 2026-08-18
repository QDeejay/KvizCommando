using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed class VsMatchEventLogEntry
{
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
    public VsMatchPhase Phase { get; init; }
    public int? PlayerId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Data { get; init; } = string.Empty;
}

/**
 * A későbbi admin-visszajátszáshoz szükséges, egyelőre memóriában
 * tartott VS eseménynapló egy bejegyzését írja le.
 */
