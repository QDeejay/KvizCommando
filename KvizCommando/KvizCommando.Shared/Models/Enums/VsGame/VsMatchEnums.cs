namespace KvizCommando.Shared.Models.Enums.VsGame;

public enum VsMatchPhase
{
    RankedQueue = 0,
    MatchLocked = 1,
    PreparationOrder = 2,
    PreparationCategories = 3,
    PreparationHelps = 4,
    PreparationCompleted = 5,
    Disconnected = 6,
    Aborted = 7
}

public enum VsHelpType
{
    None = 0,
    FiftyFifty = 1,
    GuessRange = 2,
    TimeFreeze = 3,
    AiSuggestion = 4
}

public static class VsLoadoutCategoryIds
{
    public const int MinimumFactoryCategory = 1;
    public const int MaximumFactoryCategory = 16;
    public const int OwnQuestion = 17;
    public const int AllCategories = 18;
}

/**
 * A VS meccs kliens és szerver között közösen használt fázis- és
 * segítségtípusait, valamint a speciális loadout-kategóriákat
 * tartalmazza.
 */
