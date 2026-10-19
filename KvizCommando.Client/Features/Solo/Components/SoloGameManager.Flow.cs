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
    private Task ReceiveStateAsync(SoloGameSnapshot state)
    {
        _states.Writer.TryWrite(state);
        return Task.CompletedTask;
    }

    private async Task RunGameAsync(CancellationToken ct)
    {
        try
        {
            _statusKey = "solo.Label.GameProcess.Connecting";
            await RenderAsync();
            await Audio.FadeOutMusicAsync(AudioRules.MUSIC_FADE_DURING_THE_BATTLES);
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
            _answers = Enumerable.Range(1, _game.QuestionCount)
                .Select(number => new SoloAnswerDto
                {
                    QuestionNumber = number,
                    SelectedOptionIndex = -1
                }).ToArray();
            _questions = new SoloQuestionDto?[_game.QuestionCount];
            _progress = Enumerable.Repeat(SoloQuestionState.Pending, _game.QuestionCount).ToArray();
            _points = _game.MaxPointsPerQuestion;
            _remainingSeconds = _game.AnswerTimeSeconds;
            await Audio.PlayMusicAsync(MusicTrack.BattleSolo);
            await ApplyStateAsync(_game.State, ct);

            // A szerverüzeneteket egy olvasó dolgozza fel. A SignalR fogadója
            // csak sorba állítja őket, így a renderelés nem blokkolja a kapcsolatot.
            await foreach (var state in _states.Reader.ReadAllAsync(ct))
            {
                await ApplyStateAsync(state, ct);
                if (state.Phase == SoloGamePhase.Completed)
                {
                    await FinishGameAsync(ct);
                    return;
                }
                if (state.Phase == SoloGamePhase.Abandoned)
                    return;
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during solo game.");
            await ShowFailureAsync();
        }
        finally
        {
            await StopQuestionClockAsync();
        }
    }

    private async Task ApplyStateAsync(SoloGameSnapshot state, CancellationToken ct)
    {
        await StopQuestionClockAsync();
        _answerEnabled = false;
        switch (state.Phase)
        {
            case SoloGamePhase.Preparing:
                _phase = SoloGameViewPhase.Status;
                _statusKey = "solo.Label.GameProcess.Preparing";
                break;
            case SoloGamePhase.Starting:
                _phase = SoloGameViewPhase.Status;
                _statusKey = "solo.Label.GameProcess.Starting";
                break;
            case SoloGamePhase.Question:
                _questionIndex = state.QuestionNumber - 1;
                _questions[_questionIndex] = state.Question
                    ?? throw new InvalidOperationException("Solo question payload is missing.");
                _phase = SoloGameViewPhase.Playing;
                _progress[_questionIndex] = SoloQuestionState.Active;
                _remainingSeconds = _game!.AnswerTimeSeconds;
                _points = _game.MaxPointsPerQuestion;
                _questionWatch.Restart();
                _answerEnabled = true;
                _questionCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                _questionTask = UpdateQuestionClockAsync(state.QuestionNumber, _questionCts.Token);
                break;
            case SoloGamePhase.Feedback:
                _questionIndex = state.QuestionNumber - 1;
                _answers[_questionIndex] = state.Answer
                    ?? throw new InvalidOperationException("Solo answer payload is missing.");
                _progress[_questionIndex] = SoloQuestionState.Pending;
                if (_answers[_questionIndex].SelectedOptionIndex == -1)
                {
                    _remainingSeconds = 0;
                    _points = 0;
                }
                else
                {
                    _remainingSeconds = Math.Max(0, _game!.AnswerTimeSeconds
                        - _answers[_questionIndex].AnswerTimeMs / 1000);
                    _points = CalculateAnswerPoints(_game.MaxPointsPerQuestion,
                        _answers[_questionIndex].AnswerTimeMs);
                }
                break;
            case SoloGamePhase.Finishing:
                _phase = SoloGameViewPhase.Status;
                _statusKey = "solo.Label.GameProcess.ProcessingQuestions";
                break;
            case SoloGamePhase.Completed:
                _result = state.Result
                    ?? throw new InvalidOperationException("Solo result payload is missing.");
                _answers = _result.Answers;
                break;
            case SoloGamePhase.Abandoned:
                _phase = SoloGameViewPhase.Failed;
                _statusKey = state.ErrorKey;
                break;
        }
        await RenderAsync();
    }

    private async Task UpdateQuestionClockAsync(int questionNumber, CancellationToken ct)
    {
        try
        {
            var maximumTimeMs = _game!.AnswerTimeSeconds * 1000;
            while (_answerEnabled)
            {
                ct.ThrowIfCancellationRequested();
                var elapsedMs = (int)_questionWatch.ElapsedMilliseconds;
                if (elapsedMs >= maximumTimeMs)
                {
                    _remainingSeconds = 0;
                    _points = 0;
                    await SaveAnswerAsync(questionNumber, -1, maximumTimeMs, ct);
                    return;
                }
                _remainingSeconds = Math.Max(0, _game.AnswerTimeSeconds - elapsedMs / 1000);
                _points = CalculateAnswerPoints(_game.MaxPointsPerQuestion, elapsedMs);
                await RenderAsync();
                await Task.Delay(100, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Solo question display failed.");
            await ShowFailureAsync();
            _lifetimeCts.Cancel();
        }
    }

    private async Task StopQuestionClockAsync()
    {
        _questionWatch.Stop();
        _questionCts?.Cancel();
        if (_questionTask is not null)
            await _questionTask;
        _questionTask = null;
        _questionCts?.Dispose();
        _questionCts = null;
    }

    private async Task SaveAnswerAsync(
        int questionNumber, int selectedOptionIndex, int answerTimeMs, CancellationToken ct)
    {
        _questionWatch.Stop();
        _answerEnabled = false;
        var answer = new SoloAnswerDto
        {
            QuestionNumber = questionNumber,
            SelectedOptionIndex = selectedOptionIndex,
            AnswerTimeMs = Math.Clamp(answerTimeMs, 0, _game!.AnswerTimeSeconds * 1000),
            ReportedQuestionIndexes = questionNumber == _game.QuestionCount
                ? _reportedQuestionIndexes.ToArray() : []
        };
        _answers[questionNumber - 1] = answer;
        _progress[questionNumber - 1] = SoloQuestionState.Pending;
        await RenderAsync();
        var submission = await GameService.SubmitAnswerAsync(answer, ct);
        if (submission is null ||
            (!submission.IsAccepted && submission.ErrorKey == "solo.Error.Session"))
        {
            await ShowFailureAsync();
            _lifetimeCts.Cancel();
        }
        // A szerver a határidő vagy egy korábbi válasz miatt elutasíthatja
        // a beküldést. Az elszámolt választ a következő állapot közli.
    }

    private async Task SelectAnswerAsync(int answerIndex)
    {
        if (_phase != SoloGameViewPhase.Playing || !_answerEnabled)
            return;
        var elapsedMs = (int)_questionWatch.ElapsedMilliseconds;
        var maximumTimeMs = _game!.AnswerTimeSeconds * 1000;
        // A kattintás pillanatát mérjük, nem a következő kijelzőfrissítést.
        _questionWatch.Stop();
        _answerEnabled = false;
        try
        {
            await SaveAnswerAsync(_questionIndex + 1,
                elapsedMs < maximumTimeMs ? answerIndex : -1,
                Math.Min(elapsedMs, maximumTimeMs), _lifetimeCts.Token);
            if (!_lifetimeCts.IsCancellationRequested)
                await Audio.PlaySfxAsync(AudioService.SFX_SELECT);
        }
        catch (OperationCanceledException) when (_lifetimeCts.IsCancellationRequested)
        {
        }
    }

    private void ToggleQuestionReport(int questionNumber)
    {
        if (_phase != SoloGameViewPhase.Playing || _game is null ||
            questionNumber != _questionIndex + 1 || !_questionWatch.IsRunning ||
            _questionWatch.ElapsedMilliseconds >= _game.AnswerTimeSeconds * 1000L)
            return;
        if (!_reportedQuestionIndexes.Add(_questionIndex))
            _reportedQuestionIndexes.Remove(_questionIndex);
    }

    private async Task SkipQuestionAsync()
    {
        if (_phase != SoloGameViewPhase.Playing || !_answerEnabled)
            return;
        _questionWatch.Stop();
        _answerEnabled = false;
        _remainingSeconds = 0;
        _points = 0;
        try
        {
            await SaveAnswerAsync(_questionIndex + 1, -1,
                _game!.AnswerTimeSeconds * 1000, _lifetimeCts.Token);
            await Task.Delay(SoloGameRules.SKIP_EMPTY_SFX_DELAY_MS, _lifetimeCts.Token);
            await Audio.PlaySfxAsync(AudioService.SFX_EMPTY);
        }
        catch (OperationCanceledException) when (_lifetimeCts.IsCancellationRequested)
        {
        }
    }

    private async Task FinishGameAsync(CancellationToken ct)
    {
        _phase = SoloGameViewPhase.Status;
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
        await ShowStatusAsync("solo.Label.GameProcess.Evaluating", 1000, ct);
        await ShowStatusAsync("solo.Label.GameProcess.EvaluationReady", 1000, ct);
        await EvaluateAsync(ct);
    }
}
