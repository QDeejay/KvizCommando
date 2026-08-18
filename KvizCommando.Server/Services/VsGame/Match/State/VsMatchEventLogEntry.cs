using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

/// <summary>
/// A későbbi adminisztrációs visszajátszáshoz memóriában tárolt meccsesemény.
/// A tartós eseménytárolás még nincs megvalósítva.
/// </summary>
public sealed class VsMatchEventLogEntry
{
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
    public VsMatchPhase Phase { get; init; }
    public int? PlayerId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Data { get; init; } = string.Empty;
}
