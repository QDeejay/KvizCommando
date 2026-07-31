namespace KvizCommando.Server.Services.VsGame.Match;

public sealed class VsMatchStatisticsState
{
    public int Points { get; set; }
    public double TimeSeconds { get; set; }
    public int CorrectAnswers { get; set; }
    public int QuestionsAsked { get; set; }
    public int CorrectAnswersToAskedQuestions { get; set; }
    public Dictionary<int, VsMatchCategoryStatisticsState>
        Categories { get; } = [];
    public Dictionary<int, VsMatchOwnQuestionStatisticsState>
        OwnQuestions { get; } = [];
}

public sealed class VsMatchCategoryStatisticsState
{
    public int Answered { get; set; }
    public int Correct { get; set; }
}

public sealed class VsMatchOwnQuestionStatisticsState
{
    public int Asked { get; set; }
    public int CorrectAnswers { get; set; }
}

/**
 * ÚJ FÁJL: a ranked meccs közben összegyűjtött, későbbi cache-
 * mentéshez szükséges statisztikai növekményeket tartalmazza.
 * Adatbázist és PlayerCache-t nem módosít.
 */
