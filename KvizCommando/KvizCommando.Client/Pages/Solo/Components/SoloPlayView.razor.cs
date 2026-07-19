using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Pages.Solo.Components;

public partial class SoloPlayView
{
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
}


/*
 
 
 
  <div class="kc-solo-progress">
            @for (var i = 1; i <= TotalQuestions; i++)
            {
                <div class="kc-solo-progress-item @ProgressClass(i)"
                     style="grid-column: @ProgressColumn(i)">

                    <span class="kc-solo-progress-dot">
                        @if (i < CurrentQuestion)
                        {
                            <i class="bi @ProgressIcon(i)"></i>
                        }
                    </span>

                    <span class="kc-solo-progress-number">@i</span>
                </div>
            }
        </div>
 
 
 
 
 */