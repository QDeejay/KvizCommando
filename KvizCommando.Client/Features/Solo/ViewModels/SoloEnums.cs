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
