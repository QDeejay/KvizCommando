namespace KvizCommando.Client.Pages.Solo.ViewModels
{
    public enum SoloPanelMode
    {
        Status,
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