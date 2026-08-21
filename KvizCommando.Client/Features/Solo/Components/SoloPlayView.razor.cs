using KvizCommando.Client.Features.Solo.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Enums.VsGame;
using KvizCommando.Shared.Models.Rules;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Features.Solo.Components;

public partial class SoloPlayView
{
    private const int EXCELLENT_RESPONSE_TIME_MS = 50;

    [Inject] private ILanguageService Lang { get; set; } = default!;

    [Parameter, EditorRequired]
    public SoloPlayViewData Data { get; set; } = new();

    [Parameter] public EventCallback<int> OnAnswerSelected { get; set; }
    [Parameter] public EventCallback OnSkipQuestion { get; set; }
    [Parameter] public EventCallback OnSkipEvaluation { get; set; }

    private bool IsQuestionSkip =>
        Data.Panel.Mode == SoloPanelMode.Question;

    private bool CanSkip =>
        IsQuestionSkip
            ? Data.Panel.AnswerEnabled
            : Data.Panel.Mode == SoloPanelMode.Evaluation;

    private string SkipIconClass =>
        IsQuestionSkip
            ? "bi-skip-end-fill"
            : "bi-skip-forward-fill";

    private VsConnectionQuality DisplayedConnectionQuality =>
        Data.Game.IsConnectionActive
            ? Data.Game.ConnectionQuality
            : VsConnectionQuality.Unknown;

    private string ConnectionQualityClass =>
        DisplayedConnectionQuality.ToString().ToLowerInvariant();

    private string ConnectionIconClass =>
        DisplayedConnectionQuality switch
        {
            VsConnectionQuality.Good
                when Data.Game.ResponseTimeMilliseconds <=
                     EXCELLENT_RESPONSE_TIME_MS =>
                "bi-reception-4",
            VsConnectionQuality.Good => "bi-reception-3",
            VsConnectionQuality.Medium => "bi-reception-2",
            VsConnectionQuality.Bad => "bi-reception-1",
            _ => "bi-reception-0"
        };

    private string ConnectionTitle =>
        Data.Game.IsConnectionActive
            ? $"{Data.Game.ResponseTimeMilliseconds} ms"
            : string.Empty;

    private bool HasConnectionResult =>
        Data.Game.ConnectionQuality != VsConnectionQuality.Unknown;

    private string ConnectionResultText =>
        Lang["solo.Connection.ResponseTime"].FormatSafe(
            Data.Game.ResponseTimeMilliseconds,
            Lang[ConnectionQualityTextKey(
                Data.Game.ConnectionQuality)]);

    private string ConnectionResultClass =>
        Data.Game.ConnectionQuality switch
        {
            VsConnectionQuality.Good => "good",
            VsConnectionQuality.Medium => "medium",
            VsConnectionQuality.Bad => "bad",
            _ => string.Empty
        };

    private static string ConnectionQualityTextKey(
        VsConnectionQuality quality) =>
        quality switch
        {
            VsConnectionQuality.Good => "solo.Connection.Good",
            VsConnectionQuality.Medium => "solo.Connection.Medium",
            VsConnectionQuality.Bad => "solo.Connection.Bad",
            _ => string.Empty
        };
    private double TimerPercent => Data.Game.TotalSeconds <= 0
        ? 0
        : Math.Clamp((double)Data.Game.RemainingSeconds / Data.Game.TotalSeconds * 100, 0, 100);

    private string TimerStyle =>
       $"width: {TimerPercent.ToString("0.##", CultureInfo.InvariantCulture)}%";

    private string TimerStateClass => Data.Game.RemainingSeconds switch
    {
        <= SoloGameRules.CRITICAL_REMAINING_SECONDS =>
            "kc-solo-timer-critical",
        <= SoloGameRules.WARNING_REMAINING_SECONDS =>
            "kc-solo-timer-warning",
        > SoloGameRules.POINT_DECREASE_REMAINING_SECONDS =>
            "kc-solo-timer-high",
        _ => string.Empty
    };
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
    private string GetProgressLineClass(int questionNumber)
    {
        if (questionNumber <= 1)
            return string.Empty;

        if (Data.Panel.Mode == SoloPanelMode.Question)
            return questionNumber <= Data.Game.CurrentQuestion ? "line-active" : string.Empty;

        if (Data.Panel.Mode != SoloPanelMode.Evaluation)
            return string.Empty;

        return Data.Panel.Progress.ElementAtOrDefault(questionNumber - 2) switch
        {
            SoloQuestionState.Correct => "line-correct",
            SoloQuestionState.Wrong => "line-wrong",
            SoloQuestionState.Unanswered => "line-unanswered",
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

    private Task SkipAsync() =>
        IsQuestionSkip
            ? OnSkipQuestion.InvokeAsync()
            : Data.Panel.Mode == SoloPanelMode.Evaluation
                ? OnSkipEvaluation.InvokeAsync()
                : Task.CompletedTask;
}
