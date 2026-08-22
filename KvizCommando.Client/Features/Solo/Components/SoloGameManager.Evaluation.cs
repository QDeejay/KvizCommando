using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Solo.Services;
using KvizCommando.Client.Features.Solo.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums.VsGame;
using KvizCommando.Shared.Models.Rules;
using KvizCommando.Shared.Models.User;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace KvizCommando.Client.Features.Solo.Components;

partial class SoloGameManager
{
    private async Task EvaluateAsync(CancellationToken ct)
    {
        _phase = SoloGamePhase.Evaluation;
        _points = 0;
        _evaluatedCount = 0;
        _evaluatedCorrectAnswers = 0;
        _skipSignal = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        for (_questionIndex = 0;
             _questionIndex < _result!.AnswerResults.Length;
             _questionIndex++)
        {
            SetEvaluationTime(_questionIndex);
            var questionState =
                ResolveQuestionState(_questionIndex);
            _progress[_questionIndex] = questionState;
            if (questionState == SoloQuestionState.Correct)
                _evaluatedCorrectAnswers++;
            _evaluatedCount++;
            _points += _result.TotalPoints[_questionIndex];
            await Audio.PlaySfxAsync(
                EvaluationEffect(questionState));
            await RenderAsync();

            var delay = Task.Delay(
                SoloGameRules.EVALUATION_STEP_DELAY_MS,
                ct);
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
        _evaluatedCorrectAnswers = _result.CorrectAnswers;
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

    private async Task SkipEvaluationAsync()
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _skipSignal?.TrySetResult();
    }

    private async Task ShowRewardAsync()
    {
        var totalPoints = _result?.TotalPoints.Sum() ?? 0;

        if (Mode == SoloGameMode.Orientation &&
            _player.SoloBestScore is int bestScore)
        {
            _player.SoloBestScore = Math.Max(
                bestScore,
                totalPoints);
        }

        _phase = SoloGamePhase.Reward;
        _points = totalPoints;
        await Audio.PlaySfxAsync(
            _result?.IsNewHighScore == true
                ? AudioService.SFX_NEW_RECORD
                : AudioService.SFX_RESULT);
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
        _statusKey = string.IsNullOrWhiteSpace(
            GameService.ErrorMessageKey)
                ? "solo.Label.GameProcess.Aborted"
                : GameService.ErrorMessageKey;
        _answerEnabled = false;
        await RenderAsync();
    }
}
