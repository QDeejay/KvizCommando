namespace KvizCommando.Client.Features.Solo.ViewModels
{
    public enum SoloPanelMode
    {
        Status,
        Connection,
        Question,
        Evaluation,
        Reward
    }

    public enum SoloQuestionState
    {
        Pending,
        Active,
        Correct,
        Wrong,
        Unanswered
    }
}

/**
 * MÓDOSÍTÁS: a Connection mód elkülöníti a pingteszt megjelenítését a
 * Solo általános státusz- és kérdésnézeteitől.
 */
