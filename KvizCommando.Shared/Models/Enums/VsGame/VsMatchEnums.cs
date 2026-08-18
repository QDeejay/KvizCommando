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
    Aborted = 7,
    GameStarting = 8,
    NormalRoundGuess = 9,
    NormalRoundQuestion = 10,
    QuestionResult = 11,
    NormalRoundResult = 12,
    CaptainQuestionSelection = 13,
    CaptainQuestion = 14,
    CaptainRoundResult = 15,
    GameCompleted = 16,
    PreparationStarting = 17
}

public enum VsQuestionKind
{
    None = 0,
    Guess = 1,
    Choice = 2
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
    public const int ALL_CATEGORIES = 0;
    public const int MINIMUM_FACTORY_CATEGORY = 1;
    public const int MAXIMUM_FACTORY_CATEGORY = 16;
    public const int OWN_QUESTION = 17;
}
