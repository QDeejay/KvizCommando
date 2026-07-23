using KvizCommando.Client.Pages.Solo.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Pages.Solo.Components;

public partial class SoloPlayView
{
    [Inject] private ILanguageService Lang { get; set; } = default!;

    [Parameter, EditorRequired]
    public SoloPlayViewData Data { get; set; } = new();

    [Parameter] public EventCallback<int> OnAnswerSelected { get; set; }
    [Parameter] public EventCallback OnSkipEvaluation { get; set; }
    private double TimerPercent => Data.Game.TotalSeconds <= 0
        ? 0
        : Math.Clamp((double)Data.Game.RemainingSeconds / Data.Game.TotalSeconds * 100, 0, 100);

    private string TimerStyle =>
       $"width: {TimerPercent.ToString("0.##", CultureInfo.InvariantCulture)}%";

    private string TimerStateClass => Data.Game.RemainingSeconds switch
    {
        <= 0 => "kc-solo-timer-zero",
        <= 5 => "kc-solo-timer-warning",
        > 15 => "kc-solo-timer-high",
        _ => string.Empty
    };
    private string QuestionPanelClass => Data.Panel.Mode == SoloPanelMode.Reward
        ? "kc-solo-question-text reward"
        : "kc-solo-question-text";

    private string GetAnswerClass(int answerIndex)
    {
        if (answerIndex != Data.Panel.SelectedAnswerIndex)
            return string.Empty;

        if (Data.Panel.Mode != SoloPanelMode.Evaluation)
            return "selected";

        return Data.Panel.CurrentAnswerResult == true ? "correct" : "wrong";
    }
    private string GetProgressClass(int questionNumber)
    {
        var state = Data.Panel.Progress.ElementAtOrDefault(questionNumber - 1);

        return state switch
        {
            SoloQuestionState.Active => "active",
            SoloQuestionState.Correct => "correct",
            SoloQuestionState.Wrong => "wrong",
            SoloQuestionState.Unanswered => "unanswered",
            _ => string.Empty
        };
    }
    private static string GetProgressPosition(int questionNumber) => questionNumber <= 10
        ? $"grid-row: 1; grid-column: {questionNumber}"
        : $"grid-row: 2; grid-column: {21 - questionNumber}";

    private static char AnswerLetter(int index) => (char)('A' + index);
    private Task SelectAnswerAsync(int answerIndex) => Data.Panel.AnswerEnabled
       ? OnAnswerSelected.InvokeAsync(answerIndex)
       : Task.CompletedTask;
}