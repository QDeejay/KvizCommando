namespace KvizCommando.Server.Services.VsGame.Match;

public sealed class VsMatchProfile
{
    public int RequiredPlayers { get; init; } = 2;
    public int MinimumPlayers { get; init; } = 2;
    public int MaximumPlayers { get; init; } = 4;
    public int PreparationSeconds { get; init; } = 20;
    public int GuessSeconds { get; init; } = 20;
    public int QuestionSeconds { get; init; } = 15;
    public int QuestionPauseSeconds { get; init; } = 10;
    public int RoundResultSeconds { get; init; } = 5;
    public int PhasePauseSeconds { get; init; } = 5;
    public int PointUnit { get; init; } = 1;
    public int CaptainMultiplier { get; init; } = 2;
    public int LoadoutSize { get; init; } = 10;
    public bool PausePreparationOnTimeout { get; init; } = true;
}

public static class VsMatchProfiles
{
    public static readonly VsMatchProfile Ranked = new();
}

/**
 * MÓDOSÍTÁS: a tippkérdés saját, egy helyen állítható időkeretet
 * kapott. A PausePreparationOnTimeout fejlesztői flag true értéken
 * hagyja nullára futni az órát, de nem lépteti automatikusan tovább
 * a preparációt.
 *
 * A VS meccsmotor egy helyen módosítható idő-, pont- és
 * létszámprofilját tartalmazza.
 */
