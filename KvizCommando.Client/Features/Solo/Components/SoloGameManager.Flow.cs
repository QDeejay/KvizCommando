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
    private async Task RunGameAsync(CancellationToken ct)
    {
        try
        {
            await Audio.PlayMusicAsync(
                MusicTrack.BattleSolo);
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

            _hasActiveGame = true;
            await ShowStatusAsync(
                "solo.Label.GameProcess.Preparing",
                1000,
                ct);

            _answers = [.. _game.Questions.Select(question => new SoloAnswerDto
            {
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
                await SaveAnswerAsync(
                    await _answerSignal.Task,
                    (int)_questionWatch.ElapsedMilliseconds,
                    ct);
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
        _points = 0;
        await SaveAnswerAsync(
            -1,
            _game.AnswerTimeSeconds * 1000,
            ct);
    }

    private async Task SaveAnswerAsync(
        int selectedOptionIndex,
        int answerTimeMs,
        CancellationToken ct)
    {
        _questionWatch.Stop();
        _answerEnabled = false;
        _answers[_questionIndex].SelectedOptionIndex = selectedOptionIndex;
        _answers[_questionIndex].AnswerTimeMs = Math.Clamp(
            answerTimeMs,
            0,
            _game!.AnswerTimeSeconds * 1000);
        _progress[_questionIndex] = SoloQuestionState.Pending;

        var submission = await GameService.SubmitAnswerAsync(
            _answers[_questionIndex],
            ct);

        if (submission is null || !submission.IsAccepted)
        {
            throw new InvalidOperationException(
                "Solo answer was rejected by the server.");
        }

        if (submission.Result is not null)
            _result = submission.Result;
    }

    private async Task SelectAnswerAsync(int answerIndex)
    {
        if (_phase == SoloGamePhase.Playing && _answerEnabled)
        {
            _answerEnabled = false;
            _answerSignal?.TrySetResult(answerIndex);
            await Audio.PlaySfxAsync(
                AudioService.SFX_SELECT);
        }
    }

    private async Task FinishGameAsync(CancellationToken ct)
    {
        _phase = SoloGamePhase.Status;
        _statusKey = "solo.Label.GameProcess.ProcessingQuestions";
        _points = 0;
        await RenderAsync();

        if (_result is null)
        {
            await ShowFailureAsync();
            return;
        }

        _hasActiveGame = false;
        await OnGameCompletedChanged.InvokeAsync(true);

        if (_result.Rewards.NewTeamLevel > 0)
        {
            await OnTeamLevelChanged.InvokeAsync(
                _result.Rewards.NewTeamLevel);
        }
        await Audio.PlayMusicAsync(
            MusicTrack.MenuMain);
        await ShowStatusAsync("solo.Label.GameProcess.Evaluating", 1000, ct);
        await ShowStatusAsync("solo.Label.GameProcess.EvaluationReady", 1000, ct);
        await EvaluateAsync(ct);
    }
}
