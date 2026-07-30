using KvizCommando.Client.Features.VsGame.Match.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Features.VsGame.Match.Components;

public partial class VsMatchPlayView : IDisposable
{
    [Inject] private ILanguageService Lang { get; set; } = default!;

    [Parameter, EditorRequired]
    public VsMatchViewData Data { get; set; } = new();

    [Parameter]
    public EventCallback<VsGuessAnswerRequest>
        OnGuessSubmitted { get; set; }

    [Parameter]
    public EventCallback<VsChoiceAnswerRequest>
        OnChoiceSubmitted { get; set; }

    [Parameter]
    public EventCallback<VsCaptainQuestionRequest>
        OnCaptainQuestionSelected { get; set; }

    private System.Threading.Timer? _timer;
    private double? _guessValue;
    private int _lastQuestionNumber;
    private bool _sending;

    protected override void OnInitialized()
    {
        _timer = new System.Threading.Timer(
            _ => _ = InvokeAsync(StateHasChanged),
            null,
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(250));
    }

    protected override void OnParametersSet()
    {
        if (_lastQuestionNumber ==
            Data.Game.QuestionNumber)
        {
            return;
        }

        _lastQuestionNumber =
            Data.Game.QuestionNumber;
        _guessValue = null;
        _sending = false;
    }

    private int RemainingSeconds => !Data.DeadlineUtc.HasValue
        ? 0
        : Math.Max(
            0,
            (int)Math.Ceiling(
                (Data.DeadlineUtc.Value - DateTime.UtcNow)
                .TotalSeconds));

    private double TimerPercent => Data.PhaseDurationSeconds <= 0
        ? 0
        : Math.Clamp(
            (double)RemainingSeconds /
            Data.PhaseDurationSeconds * 100,
            0,
            100);

    private string TimerStyle =>
        $"width: {TimerPercent.ToString(
            "0.##",
            CultureInfo.InvariantCulture)}%";

    private string WaitTimerStyle =>
        $"--kc-vs-wait-progress: {TimerPercent.ToString(
            "0.##",
            CultureInfo.InvariantCulture)}%";

    private bool ShowAnswerTimer =>
        Data.DeadlineUtc.HasValue &&
        Data.Phase is
            VsMatchPhase.NormalRoundGuess or
            VsMatchPhase.NormalRoundQuestion or
            VsMatchPhase.CaptainQuestionSelection or
            VsMatchPhase.CaptainQuestion;

    private bool ShowWaitTimer =>
        Data.DeadlineUtc.HasValue &&
        Data.Phase is
            VsMatchPhase.GameStarting or
            VsMatchPhase.QuestionResult or
            VsMatchPhase.NormalRoundResult or
            VsMatchPhase.CaptainRoundResult;

    private bool ShowProgress =>
        Data.Game.CurrentRoundNumber > 0 &&
        Data.Phase != VsMatchPhase.GameCompleted;

    private string MatchReference =>
        Data.MatchId.ToString("N")[..8]
            .ToUpperInvariant();

    private string RoundText =>
        Data.Game.CurrentRoundNumber <= 0
            ? Data.ClassificationText
            : Data.Game.CurrentRoundNumber >
        Data.Game.NormalRoundCount
            ? Lang["vsgame.Match.Round.Captain"]
            : Lang["vsgame.Match.Round.Normal"]
                .FormatSafe(
                    Data.Game.CurrentRoundNumber);

    private bool CanSubmitGuess =>
        Data.Game.CanAnswer &&
        _guessValue.HasValue &&
        double.IsFinite(_guessValue.Value) &&
        !_sending;

    private VsHelpCardVm? CurrentHelp =>
        Data.Preparation.Rounds
            .FirstOrDefault(round =>
                round.RoundNumber ==
                Data.Game.CurrentRoundNumber)
            ?.Help;

    private int MyRoundPoints =>
        Data.Game.MyRoundPoints;

    private double MyRoundTime =>
        Data.Game.MyRoundTimeSeconds;

    private string PlayerName(int position) =>
        Data.Players
            .FirstOrDefault(player =>
                player.Position == position)
            ?.TeamName ?? position.ToString();

    private static string Signed(int value) =>
        value > 0 ? $"+{value}" : value.ToString();

    private static string Seconds(double value) =>
        $"{value:0.0}s";

    private string AnswerClass(int answerIndex)
    {
        if (!Data.Game.CorrectAnswerIndex.HasValue)
        {
            return Data.Game.MyAnswerIndex == answerIndex
                ? "selected"
                : string.Empty;
        }

        if (Data.Game.CorrectAnswerIndex == answerIndex)
            return "correct";

        return Data.Game.MyAnswerIndex == answerIndex
            ? "wrong"
            : string.Empty;
    }

    private async Task SubmitGuessAsync()
    {
        if (!CanSubmitGuess)
            return;

        _sending = true;

        try
        {
            await OnGuessSubmitted.InvokeAsync(
                new VsGuessAnswerRequest
                {
                    QuestionNumber =
                        Data.Game.QuestionNumber,
                    Value = _guessValue!.Value
                });
        }
        finally
        {
            _sending = false;
        }
    }

    private async Task SubmitChoiceAsync(int answerIndex)
    {
        if (!Data.Game.CanAnswer || _sending)
            return;

        _sending = true;

        try
        {
            await OnChoiceSubmitted.InvokeAsync(
                new VsChoiceAnswerRequest
                {
                    QuestionNumber =
                        Data.Game.QuestionNumber,
                    AnswerIndex = answerIndex
                });
        }
        finally
        {
            _sending = false;
        }
    }

    private async Task SelectCaptainQuestionAsync(
        int loadoutPosition)
    {
        if (!Data.Game.CanChooseCaptainQuestion ||
            _sending)
        {
            return;
        }

        _sending = true;

        try
        {
            await OnCaptainQuestionSelected.InvokeAsync(
                new VsCaptainQuestionRequest
                {
                    LoadoutPosition = loadoutPosition
                });
        }
        finally
        {
            _sending = false;
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/**
 * ÚJ FÁJL: a VS játéknézet visszaszámlálását, lokális beviteli
 * állapotát és a három explicit EventCallback parancsát kezeli.
 * Pontot, sorrendet vagy válaszjogosultságot nem számol kliensoldalon.
 * A teljes MatchId-ből csak megjelenítési célú rövid hivatkozást képez.
 * MÓDOSÍTÁS: külön kezeli a válaszadási idősort és a színezés nélküli,
 * kör alakú várakozási visszaszámlálót.
 */
