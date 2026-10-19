using System.Diagnostics;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.SoloGame.GameCache;
using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Models.Rules;

namespace KvizCommando.Server.Services.SoloGame;

public sealed partial class SoloGameService
{
    /// <summary>
    /// Egyetlen szerveroldali folyamat lépteti a játékot. A válaszbeküldés csak
    /// rögzíti a választ és jelzi a kérdés lezárhatóságát.
    /// </summary>
    private async Task RunGameAsync(SoloGameSession game)
    {
        var ct = game.LifetimeCts.Token;
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(SoloGameRules.PREPARATION_SECONDS), ct);
            await ChangePhaseAsync(game, SoloGamePhase.Starting,
                TimeSpan.FromSeconds(SoloGameRules.STARTING_SECONDS), ct);
            await Task.Delay(TimeSpan.FromSeconds(SoloGameRules.STARTING_SECONDS), ct);

            for (var index = 0; index < game.Questions.Count; index++)
            {
                await game.Lock.WaitAsync(ct);
                SoloGameSnapshot questionState;
                Task answerCompletion;
                try
                {
                    EnsureActive(game, ct);
                    game.QuestionIndex = index;
                    game.QuestionCompletion = new TaskCompletionSource(
                        TaskCreationOptions.RunContinuationsAsynchronously);
                    SetPhase(game, SoloGamePhase.Question,
                        TimeSpan.FromSeconds(SoloGameRules.ANSWER_SECONDS));
                    answerCompletion = game.QuestionCompletion.Task;
                    questionState = CreateSnapshot(game);
                }
                finally
                {
                    game.Lock.Release();
                }

                // Az időmérés a küldés előtt indul. A küldés ideje is része
                // a szervermérésnek, és nem tolhatja ki a válasz határidejét.
                await SendStateAsync(game, questionState, ct);
                var remainingMs = SoloGameRules.ANSWER_SECONDS * 1000
                    + SoloGameRules.ANSWER_RECEIVE_GRACE_MS
                    - Stopwatch.GetElapsedTime(game.PhaseStartedTimestamp).TotalMilliseconds;
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var timeout = Task.Delay(TimeSpan.FromMilliseconds(Math.Max(0, remainingMs)), timeoutCts.Token);
                await Task.WhenAny(answerCompletion, timeout);
                ct.ThrowIfCancellationRequested();
                timeoutCts.Cancel();

                SoloGameSnapshot feedbackState;
                await game.Lock.WaitAsync(ct);
                try
                {
                    EnsureActive(game, ct);
                    if (game.Answers.Count == index)
                    {
                        game.Answers.Add(new SoloAnswerDto
                        {
                            QuestionNumber = index + 1,
                            SelectedOptionIndex = -1,
                            AnswerTimeMs = SoloGameRules.ANSWER_SECONDS * 1000
                        });
                    }
                    SetPhase(game, SoloGamePhase.Feedback,
                        TimeSpan.FromSeconds(SoloGameRules.FEEDBACK_SECONDS));
                    feedbackState = CreateSnapshot(game);
                }
                finally
                {
                    game.Lock.Release();
                }
                await SendStateAsync(game, feedbackState, ct);
                await Task.Delay(TimeSpan.FromSeconds(SoloGameRules.FEEDBACK_SECONDS), ct);
            }

            await ChangePhaseAsync(game, SoloGamePhase.Finishing, null, ct);
            SoloGameSnapshot completedState;
            await game.Lock.WaitAsync(ct);
            try
            {
                var result = await CompleteAsync(game, ct);
                if (result.Status != SoloGameOperationStatus.Success || result.Response is null)
                    throw new InvalidOperationException("Solo result could not be applied to the player cache.");

                if (game.ReportedQuestionIndexes.Length > 0)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var playerCache = scope.ServiceProvider.GetRequiredService<IPlayerCacheService>();
                    await playerCache.UpdateRewardQuestionsLockedAsync(
                        game.PlayerId, game.SessionId,
                        (_, questions) =>
                        {
                            foreach (var index in game.ReportedQuestionIndexes)
                                questions.rfSlots.Add(game.Questions[index].QuestionId);
                            return 1u << 21;
                        }, ct);
                }
                SetPhase(game, SoloGamePhase.Completed, null);
                completedState = CreateSnapshot(game);
                completedState.Result = result.Response;
            }
            finally
            {
                game.Lock.Release();
            }
            // A végleges eredmény nem egy kliensbeküldés visszatérési értéke:
            // az utolsó kérdés szerveroldali lejáratából is létrejöhet.
            await SendStateAsync(game, completedState, ct);
            _gameCache.Remove(game.GameId);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            await AbandonAndNotifyAsync(game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Solo game failed. GameId={GameId}", game.GameId);
            await AbandonAndNotifyAsync(game);
        }
    }

    private async Task AbandonAndNotifyAsync(SoloGameSession game)
    {
        SoloGameSnapshot? failedState = null;
        await game.Lock.WaitAsync();
        try
        {
            if (game.Phase != SoloGamePhase.Completed)
            {
                SetPhase(game, SoloGamePhase.Abandoned, null);
                failedState = CreateSnapshot(game);
                failedState.ErrorKey = "solo.Label.GameProcess.Aborted";
            }
        }
        finally
        {
            game.Lock.Release();
        }
        _gameCache.Remove(game.GameId);
        if (failedState is null)
            return;
        try
        {
            await _hub.Clients.Client(game.ConnectionId).StateChanged(failedState)
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Solo failure notification could not be delivered.");
        }
    }

    private async Task ChangePhaseAsync(
        SoloGameSession game,
        SoloGamePhase phase,
        TimeSpan? duration,
        CancellationToken ct)
    {
        SoloGameSnapshot state;
        await game.Lock.WaitAsync(ct);
        try
        {
            EnsureActive(game, ct);
            SetPhase(game, phase, duration);
            state = CreateSnapshot(game);
        }
        finally
        {
            game.Lock.Release();
        }
        await SendStateAsync(game, state, ct);
    }

    private static void EnsureActive(SoloGameSession game, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (game.Phase is SoloGamePhase.Completed or SoloGamePhase.Abandoned)
            throw new OperationCanceledException(ct);
    }

    private static void SetPhase(SoloGameSession game, SoloGamePhase phase, TimeSpan? duration)
    {
        game.Phase = phase;
        game.PhaseStartedTimestamp = Stopwatch.GetTimestamp();
        game.DeadlineUtc = duration.HasValue ? DateTime.UtcNow.Add(duration.Value) : null;
    }

    private static SoloGameSnapshot CreateSnapshot(SoloGameSession game) => new()
    {
        Phase = game.Phase,
        QuestionNumber = game.Phase is SoloGamePhase.Question or SoloGamePhase.Feedback
            ? game.QuestionIndex + 1 : 0,
        DeadlineUtc = game.DeadlineUtc,
        Question = game.Phase == SoloGamePhase.Question
            ? new SoloQuestionDto
            {
                Question = game.Questions[game.QuestionIndex].Question,
                Answers = game.Questions[game.QuestionIndex].Answers.ToArray()
            }
            : null,
        Answer = game.Phase == SoloGamePhase.Feedback
            ? game.Answers[game.QuestionIndex] : null
    };

    private Task SendStateAsync(SoloGameSession game, SoloGameSnapshot state, CancellationToken ct) =>
        _hub.Clients.Client(game.ConnectionId).StateChanged(state).WaitAsync(ct);
}
