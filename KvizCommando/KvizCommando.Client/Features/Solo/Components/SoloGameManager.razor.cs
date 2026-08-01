using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Solo.Services;
using KvizCommando.Client.Features.Solo.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.User;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace KvizCommando.Client.Features.Solo.Components;

public partial class SoloGameManager : IAsyncDisposable
{
    [Inject] private ISoloGameClientService GameService { get; set; } = default!;
    [Inject] private AudioService Audio { get; set; } = default!;
    [Inject] private UiServices Ui { get; set; } = default!;
    [Inject] private ILogger<SoloGameManager> Logger { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    [Parameter] public SoloGameMode Mode { get; set; }
    [Parameter] public int SelectionId { get; set; }
    [Parameter] public string Title { get; set; } = string.Empty;

    private readonly CancellationTokenSource _lifetimeCts = new();
    private readonly Stopwatch _gameWatch = new();
    private readonly Stopwatch _questionWatch = new();

    private SoloPlayerViewData _player = new();
    private StartSoloGameResponse? _game;
    private FinishSoloGameResponse? _result;
    private SoloAnswerDto[] _answers = [];
    private SoloQuestionState[] _progress = [];
    private TaskCompletionSource<int>? _answerSignal;
    private TaskCompletionSource? _skipSignal;
    private Task? _gameTask;
    private Guid? _activeGameId;
    private SoloGamePhase _phase = SoloGamePhase.Status;
    private int _questionIndex;
    private int _remainingSeconds;
    private int _points;
    private int _evaluatedCount;
    private bool _answerEnabled;
    private bool _isDisposed;
    private string _statusKey = "solo.Label.GameProcess.Preparing";

    private string Culture => AppStates.Culture;
    private UserMainData UserData => AppStates.Home!.UserMainData;
    private TeamMemberDto[] Members => AppStates.Team!.TeamMembers;

    private SoloPlayViewData PlayViewData => new()
    {
        Player = _player,
        Game = new SoloGameViewData
        {
            Title = Title,
            Points = _points,
            CurrentQuestion = _game is null
                ? 0
                : Math.Min(_questionIndex + 1, _game.QuestionCount),
            TotalQuestions = _game?.QuestionCount ?? 0,
            TotalSeconds = _game?.AnswerTimeSeconds ?? 0,
            RemainingSeconds = _remainingSeconds
        },
        Panel = BuildPanelData()
    };

    protected override void OnInitialized()
    {
        _player = BuildPlayerProfile();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            _gameTask = RunGameAsync(_lifetimeCts.Token);
    }

    private async Task RunGameAsync(CancellationToken ct)
    {
        try
        {
            await Audio.PlayMusicAsync("Battle01.webm");
            await ShowStatusAsync("solo.Label.GameProcess.Preparing", 1000, ct);

            _statusKey = "solo.Label.GameProcess.Connecting";
            await RenderAsync();

            _game = await GameService.StartAsync(new StartSoloGameRequest
            {
                Mode = Mode,
                SelectionId = SelectionId
            }, ct);

            if (_game is null)
            {
                await ShowFailureAsync();
                return;
            }

            _activeGameId = _game.GameId;
            _answers = [.. _game.Questions.Select(question => new SoloAnswerDto
            {
                QuestionToken = question.QuestionToken,
                SelectedOptionIndex = -1
            })];
            _progress = [.. Enumerable.Repeat(
                SoloQuestionState.Pending,
                _game.QuestionCount)];
            _points = _game.MaxPointsPerQuestion;
            _remainingSeconds = _game.AnswerTimeSeconds;

            await ShowStatusAsync("solo.Label.GameProcess.Starting", 1000, ct);
            await PlayAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during solo game.");
            await ShowFailureAsync();
        }
    }

    private async Task PlayAsync(CancellationToken ct)
    {
        _phase = SoloGamePhase.Playing;
        _gameWatch.Restart();

        for (_questionIndex = 0; _questionIndex < _answers.Length; _questionIndex++)
        {
            _progress[_questionIndex] = SoloQuestionState.Active;
            await PlayQuestionAsync(ct);

            _answerEnabled = false;
            await RenderAsync();
            await Task.Delay(
                TimeSpan.FromSeconds(_game!.FeedbackTimeSeconds),
                ct);
        }

        _gameWatch.Stop();
        await FinishGameAsync(ct);
    }

    private async Task PlayQuestionAsync(CancellationToken ct)
    {
        _answerSignal = new TaskCompletionSource<int>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        _questionWatch.Restart();
        _remainingSeconds = _game!.AnswerTimeSeconds;
        _points = _game.MaxPointsPerQuestion;
        _answerEnabled = true;
        await RenderAsync();

        while (_questionWatch.ElapsedMilliseconds < _game.AnswerTimeSeconds * 1000L)
        {
            if (_answerSignal.Task.IsCompleted)
            {
                SaveAnswer(
                    await _answerSignal.Task,
                    (int)_questionWatch.ElapsedMilliseconds);
                return;
            }

            _remainingSeconds = Math.Max(
                0,
                _game.AnswerTimeSeconds -
                (int)(_questionWatch.ElapsedMilliseconds / 1000));
            _points = Math.Max(
                CalculateAnswerPoints(
                    _game.MaxPointsPerQuestion,
                    (int)_questionWatch.ElapsedMilliseconds),
                0);

            await RenderAsync();
            await Task.Delay(100, ct);
        }

        _remainingSeconds = 0;
        SaveAnswer(-1, _game.AnswerTimeSeconds * 1000);
    }

    private void SaveAnswer(int selectedOptionIndex, int answerTimeMs)
    {
        _questionWatch.Stop();
        _answerEnabled = false;
        _answers[_questionIndex].SelectedOptionIndex = selectedOptionIndex;
        _answers[_questionIndex].AnswerTimeMs = Math.Clamp(
            answerTimeMs,
            0,
            _game!.AnswerTimeSeconds * 1000);
        _progress[_questionIndex] = SoloQuestionState.Pending;
    }

    private Task SelectAnswerAsync(int answerIndex)
    {
        if (_phase == SoloGamePhase.Playing && _answerEnabled)
        {
            _answerEnabled = false;
            _answerSignal?.TrySetResult(answerIndex);
        }

        return Task.CompletedTask;
    }

    private async Task FinishGameAsync(CancellationToken ct)
    {
        _phase = SoloGamePhase.Status;
        _statusKey = "solo.Label.GameProcess.ProcessingQuestions";
        _points = 0;
        await RenderAsync();

        _result = await GameService.FinishAsync(
            _game!.GameId,
            new FinishSoloGameRequest
            {
                ClientElapsedMs = (int)_gameWatch.ElapsedMilliseconds,
                Answers = _answers
            },
            ct);

        if (_result is null)
        {
            await ShowFailureAsync();
            return;
        }

        _activeGameId = null;
        await Ui.ReloadAsync(
            ReqStates.Home,
            ReqStates.Team,
            ReqStates.SoloGame);
        await Audio.PlayMusicAsync("Menu02.webm");
        await ShowStatusAsync("solo.Label.GameProcess.Evaluating", 1000, ct);
        await ShowStatusAsync("solo.Label.GameProcess.EvaluationReady", 1000, ct);
        await EvaluateAsync(ct);
    }

    private async Task EvaluateAsync(CancellationToken ct)
    {
        _phase = SoloGamePhase.Evaluation;
        _points = 0;
        _evaluatedCount = 0;
        _skipSignal = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        for (_questionIndex = 0;
             _questionIndex < _result!.AnswerResults.Length;
             _questionIndex++)
        {
            SetEvaluationTime(_questionIndex);
            _progress[_questionIndex] = ResolveQuestionState(_questionIndex);
            _evaluatedCount++;
            _points += _result.TotalPoints[_questionIndex];
            await RenderAsync();

            var delay = Task.Delay(1000, ct);
            if (await Task.WhenAny(delay, _skipSignal.Task) == _skipSignal.Task)
                break;
        }

        CompleteEvaluationProgress();
        await RenderAsync();
        await Task.Delay(700, ct);
        await ShowRewardAsync();
    }

    private SoloQuestionState ResolveQuestionState(int questionIndex)
    {
        if (_answers[questionIndex].SelectedOptionIndex == -1)
            return SoloQuestionState.Unanswered;

        return _result!.AnswerResults[questionIndex]
            ? SoloQuestionState.Correct
            : SoloQuestionState.Wrong;
    }

    private void CompleteEvaluationProgress()
    {
        for (var i = _evaluatedCount; i < _result!.AnswerResults.Length; i++)
            _progress[i] = ResolveQuestionState(i);

        _evaluatedCount = _result.AnswerResults.Length;
        _points = _result.TotalPoints.Sum();
        _questionIndex = Math.Max(_result.AnswerResults.Length - 1, 0);
    }

    private void SetEvaluationTime(int questionIndex)
    {
        var elapsedSeconds = _answers[questionIndex].AnswerTimeMs / 1000;

        _remainingSeconds = Math.Max(
            _game!.AnswerTimeSeconds - elapsedSeconds,
            0);
    }

    private Task SkipEvaluationAsync()
    {
        _skipSignal?.TrySetResult();
        return Task.CompletedTask;
    }

    private async Task ShowRewardAsync()
    {
        _phase = SoloGamePhase.Reward;
        _points = _result?.TotalPoints.Sum() ?? 0;
        await RenderAsync();
    }

    private async Task ShowStatusAsync(
        string resourceKey,
        int delayMs,
        CancellationToken ct)
    {
        _phase = SoloGamePhase.Status;
        _statusKey = resourceKey;
        await RenderAsync();
        await Task.Delay(delayMs, ct);
    }

    private async Task ShowFailureAsync()
    {
        _phase = SoloGamePhase.Failed;
        _statusKey = "solo.Label.GameProcess.Aborted";
        _answerEnabled = false;
        await RenderAsync();
    }

    private SoloPanelViewData BuildPanelData()
    {
        if (_phase == SoloGamePhase.Reward)
            return BuildRewardPanel();

        if (_phase is SoloGamePhase.Status or SoloGamePhase.Failed)
        {
            return new SoloPanelViewData
            {
                Mode = SoloPanelMode.Status,
                DisplayLines =
                [
                    new SoloDisplayLine { ResourceKey = _statusKey }
                ],
                Progress = _progress
            };
        }

        var question = _game?.Questions.ElementAtOrDefault(_questionIndex);

        return new SoloPanelViewData
        {
            Mode = _phase == SoloGamePhase.Evaluation
                ? SoloPanelMode.Evaluation
                : SoloPanelMode.Question,
            DisplayLines = question is null
                ? []
                : [new SoloDisplayLine { Text = question.Question }],
            Answers = question?.Answers ?? [],
            Progress = _progress,
            SelectedAnswerIndex =
                _answers.ElementAtOrDefault(_questionIndex)?.SelectedOptionIndex ?? -1,
            CurrentAnswerResult =
                _phase == SoloGamePhase.Evaluation &&
                _questionIndex < _evaluatedCount
                    ? _result?.AnswerResults[_questionIndex]
                    : null,
            AnswerEnabled = _answerEnabled
        };
    }

    private SoloPanelViewData BuildRewardPanel()
    {
        var answered = _answers.Count(answer => answer.SelectedOptionIndex >= 0);
        var totalPoints = _result?.TotalPoints.Sum() ?? 0;

        var lines = new List<SoloDisplayLine>
        {
            new()
            {
                ResourceKey = "solo.Label.Game.Reward.Answered",
                Value = $"{answered} / {_answers.Length}"
            },
            new()
            {
                ResourceKey = "solo.Label.Game.Reward.Correct",
                Value = (_result?.CorrectAnswers ?? 0).ToString()
            },
            new()
            {
                ResourceKey = "solo.Label.Game.Reward.Time",
                Value = FormatTime(_result?.TotalAnswerTimeMs ?? 0)
            },
            new()
            {
                ResourceKey = "solo.Label.Game.Reward.TotalPoints",
                Value = totalPoints.ToString()
            }
        };

        if (_result?.IsNewHighScore == true)
        {
            lines.Add(new SoloDisplayLine
            {
                ResourceKey = "solo.Label.Game.Message.NewRecord",
                Emphasized = true
            });
        }

        AddRewardLine(
            lines,
            "solo.Label.Game.Reward.TeamXp",
            _result?.Rewards.TeamXp ?? 0);
        AddRewardLine(
            lines,
            "solo.Label.Game.Reward.TeamDev",
            _result?.Rewards.TeamDevPoints ?? 0);
        AddRewardLine(
            lines,
            "solo.Label.Game.Reward.MemberXp",
            _result?.Rewards.MemberXp ?? 0);
        AddRewardLine(
            lines,
            "solo.Label.Game.Reward.MemberDev",
            _result?.Rewards.MemberDevPoints ?? 0);

        return new SoloPanelViewData
        {
            Mode = SoloPanelMode.Reward,
            DisplayLines = lines,
            Progress = _progress
        };
    }

    private SoloPlayerViewData BuildPlayerProfile()
    {
        if (Mode == SoloGameMode.Category)
        {
            return new SoloPlayerViewData
            {
                Name = UserData.UserName,
                RankName = RankNameLocalizer.GetName(
                    UserData.RankEnum,
                    Culture),
                Level = RankNameTable.Data[UserData.RankEnum].PublicLevel
                    ?? "N/A",
                OrientationName = OrientationLocalizer.GetOrientation(
                    9,
                    Culture),
                ImageSrc = "images/avatars/basic.webp"
            };
        }

        var member = Members[SelectionId];

        return new SoloPlayerViewData
        {
            Name = member.Name,
            RankName = RankNameLocalizer.GetName(member.Level, Culture),
            Level = RankNameTable.Data[member.Level].PublicLevel ?? "N/A",
            OrientationName = OrientationLocalizer.GetOrientation(
                SelectionId,
                Culture),
            PictureCode = member.PictureCode
        };
    }

    private Task RenderAsync() =>
        _isDisposed
            ? Task.CompletedTask
            : InvokeAsync(StateHasChanged);

    private static void AddRewardLine(
        ICollection<SoloDisplayLine> lines,
        string resourceKey,
        int value)
    {
        if (value != 0)
        {
            lines.Add(new SoloDisplayLine
            {
                ResourceKey = resourceKey,
                Value = value.ToString()
            });
        }
    }

    private static int CalculateAnswerPoints(
        int maximumPoints,
        int elapsedMs)
    {
        var decreasingTimeMs = Math.Clamp(elapsedMs - 5000, 0, 15000);
        var multiplier = 1.0 - decreasingTimeMs / 15000.0;

        return (int)Math.Round(
            maximumPoints * multiplier,
            MidpointRounding.AwayFromZero);
    }

    private static string FormatTime(int milliseconds) =>
        TimeSpan.FromMilliseconds(milliseconds).ToString(@"mm\:ss");

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _answerEnabled = false;
        _lifetimeCts.Cancel();
        _answerSignal?.TrySetCanceled();
        _skipSignal?.TrySetResult();

        if (_gameTask is not null)
            await _gameTask;

        var activeGameId = _activeGameId;
        _activeGameId = null;

        if (activeGameId.HasValue)
            await GameService.AbandonAsync(activeGameId.Value);

        try
        {
            await Audio.PlayMusicAsync("Menu02.webm");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to restore menu music.");
        }

        _lifetimeCts.Dispose();
        GC.SuppressFinalize(this);
    }

    private enum SoloGamePhase
    {
        Status,
        Playing,
        Evaluation,
        Reward,
        Failed
    }
}
