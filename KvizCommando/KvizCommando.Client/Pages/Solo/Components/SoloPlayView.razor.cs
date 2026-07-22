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


/*
 
  [Inject] private ILanguageService Lang { get; set; } = default!;

    [Parameter] public string PlayerName { get; set; } = string.Empty;
    [Parameter] public string RankName { get; set; } = string.Empty;
    [Parameter] public int Level { get; set; }

    [Parameter] public IReadOnlyList<bool> AnswerResults { get; set; } = [true, false, true];
    [Parameter] public string OrientationName { get; set; } = string.Empty;
    [Parameter] public RenderFragment? PlayerImage { get; set; }
    [Parameter] public string PlayerImageSrc { get; set; } = string.Empty;
    [Parameter] public int Points { get; set; }

    [Parameter] public string CategoryName { get; set; } = string.Empty;
    [Parameter] public int CurrentQuestion { get; set; } = 1;
    [Parameter] public int TotalQuestions { get; set; } = 10;
    [Parameter] public int TotalSeconds { get; set; } = 20;
    [Parameter] public int RemainingSeconds { get; set; } = 20;
    [Parameter] public string Question { get; set; } = string.Empty;
    [Parameter] public IReadOnlyList<string> Answers { get; set; } = [];
    [Parameter] public bool AnswerEnabled { get; set; } = true;
    [Parameter] public EventCallback<int> OnAnswerSelected { get; set; }

    private double TimerPercent => TotalSeconds <= 0
        ? 0
        : Math.Clamp((double)RemainingSeconds / TotalSeconds * 100, 0, 100);

    private string TimerStyle =>
        $"width: {TimerPercent.ToString("0.##", CultureInfo.InvariantCulture)}%";

    private string TimerWarningClass => RemainingSeconds <= 5
        ? "kc-solo-timer-warning"
        : string.Empty;
    private string GetProgressClass(int questionNumber)
    {
        var resultIndex = questionNumber - 1;

        if (resultIndex < AnswerResults.Count)
        {
            return AnswerResults[resultIndex]
                ? "correct"
                : "wrong";
        }

        if (questionNumber == CurrentQuestion)
            return "active";

        return string.Empty;
    }

    private static string GetProgressPosition(int questionNumber)
    {
        if (questionNumber <= 10)
            return $"grid-row: 1; grid-column: {questionNumber}";

        return $"grid-row: 2; grid-column: {21 - questionNumber}";
    }

    private static char AnswerLetter(int index) => (char)('A' + index);


    private Task SelectAnswerAsync(int answerIndex)
    {
        if (!AnswerEnabled)
            return Task.CompletedTask;

        return OnAnswerSelected.InvokeAsync(answerIndex);
    }
 

 
 
 */