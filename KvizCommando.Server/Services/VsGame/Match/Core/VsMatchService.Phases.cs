using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed partial class VsMatchService
{
    private void StartPhaseLocked(
        VsMatchSession match,
        VsMatchPhase phase)
    {
        if (phase == VsMatchPhase.PreparationCompleted)
        {
            AddLog(
                match,
                null,
                "PreparationCompleted",
                string.Empty);

            foreach (var player in match.Players)
                player.IsFinished = !player.IsConnected;

            phase = VsMatchPhase.GameStarting;
        }

        match.PhaseTimerCts.Cancel();
        match.PhaseTimerCts.Dispose();
        match.PhaseTimerCts = new CancellationTokenSource();
        match.Phase = phase;
        match.PhaseStartedUtc = DateTime.UtcNow;

        if (IsPreparationPhase(phase))
        {
            VsMatchPreparationRules.BeginPhase(match, phase);
        }

        if (phase == VsMatchPhase.PreparationHelps &&
            match.Players.All(player => player.IsFinished))
        {
            StartPhaseLocked(
                match,
                VsMatchPhase.PreparationCompleted);
            return;
        }

        var durationSeconds =
            ResolvePhaseDuration(match, phase);

        if (durationSeconds <= 0)
        {
            match.DeadlineUtc = null;
            AddLog(match, null, "PhaseStarted", phase.ToString());
            return;
        }

        match.DeadlineUtc =
            match.PhaseStartedUtc.AddSeconds(durationSeconds);

        AddLog(match, null, "PhaseStarted", phase.ToString());

        var timerDeadlineUtc =
            IsAnswerPhase(phase)
            ? match.DeadlineUtc.Value.AddSeconds(
                match.Profile.AnswerRevealDelaySeconds)
            : match.DeadlineUtc.Value;

        _ = RunPhaseTimerAsync(
            match.MatchId,
            timerDeadlineUtc,
            match.PhaseTimerCts.Token);

        ScheduleBotActionsLocked(match);

        if (IsAnswerPhase(phase) &&
            VsMatchGameRules.HaveAllParticipantsAnswered(match))
        {
            StartAnswerResultDelayLocked(match);
        }
    }

    private async Task RunPhaseTimerAsync(
        Guid matchId,
        DateTime deadlineUtc,
        CancellationToken ct)
    {
        try
        {
            var delay = deadlineUtc - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, ct);

            if (ct.IsCancellationRequested)
                return;

            if (!_store.TryGet(matchId, out var match) ||
                match is null)
            {
                return;
            }

            (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;
            VsMatchRewardState? rewardToSave;

            lock (match.SyncRoot)
            {
                if (ct.IsCancellationRequested ||
                    match.IsClosed)
                    return;

                if (IsPreparationPhase(match.Phase) &&
                    match.Profile.PausePreparationOnTimeout)
                {
                    AddLog(
                        match,
                        null,
                        "PreparationTimerPaused",
                        match.Phase.ToString());
                    return;
                }

                rewardToSave = HandlePhaseTimeoutLocked(match);

                messages =
                    VsMatchSnapshotBuilder.BuildMessages(match);
            }

            if (rewardToSave is not null)
            {
                await _rewardPersistence.SaveAsync(
                    match.MatchId,
                    match.Players.Count,
                    rewardToSave);
            }

            try
            {
                await SendBroadcastMessagesAsync(messages);
            }
            finally
            {
                ReleaseCompletedBots(match);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "VS phase timer failed. matchId={MatchId}",
                matchId);
        }
    }

    private void AdvanceIfReadyLocked(VsMatchSession match)
    {
        var nextPhase =
            VsMatchPreparationRules.GetNextPhase(match);

        if (nextPhase.HasValue)
            StartPhaseLocked(match, nextPhase.Value);
    }

    private VsMatchRewardState? HandlePhaseTimeoutLocked(
        VsMatchSession match)
    {
        switch (match.Phase)
        {
            case VsMatchPhase.PreparationStarting:
                StartPhaseLocked(
                    match,
                    VsMatchPhase.PreparationOrder);
                break;

            case VsMatchPhase.PreparationOrder:
            case VsMatchPhase.PreparationCategories:
            case VsMatchPhase.PreparationHelps:
                FinishPreparationPhaseLocked(match);
                break;

            case VsMatchPhase.GameStarting:
                VsMatchGameRules.BeginFirstNormalRound(match);
                StartPhaseLocked(
                    match,
                    VsMatchPhase.NormalRoundGuess);
                break;

            case VsMatchPhase.NormalRoundGuess:
            case VsMatchPhase.NormalRoundQuestion:
            case VsMatchPhase.CaptainQuestion:
                CloseQuestionLocked(match);
                break;

            case VsMatchPhase.QuestionResult:
                ContinueAfterQuestionResultLocked(match);
                break;

            case VsMatchPhase.NormalRoundResult:
                ContinueAfterNormalRoundLocked(match);
                break;

            case VsMatchPhase.CaptainQuestionSelection:
                VsMatchGameRules
                    .SelectDefaultCaptainQuestion(match);
                StartCaptainQuestionDelayLocked(match);
                break;

            case VsMatchPhase.CaptainRoundResult:
                VsMatchGameRules.CommitRoundResult(match);
                return CompleteMatchLocked(match);
        }

        return null;
    }

    private void FinishPreparationPhaseLocked(
        VsMatchSession match)
    {
        foreach (var player in match.Players
                     .Where(player => !player.IsFinished))
        {
            VsMatchPreparationRules.ApplyTimeoutDefaults(
                match,
                player);
            player.IsFinished = true;
            AddLog(
                match,
                player.PlayerId,
                "PreparationTimeout",
                match.Phase.ToString());
        }

        AdvanceIfReadyLocked(match);
    }

    private void CloseQuestionLocked(VsMatchSession match)
    {
        VsMatchGameRules.CloseCurrentQuestion(match);

        AddLog(
            match,
            null,
            "QuestionClosed",
            $"Question={match.Game.QuestionNumber}");

        StartPhaseLocked(
            match,
            VsMatchPhase.QuestionResult);
    }

    private void StartAnswerResultDelayLocked(
        VsMatchSession match)
    {
        match.PhaseTimerCts.Cancel();
        match.PhaseTimerCts.Dispose();
        match.PhaseTimerCts = new CancellationTokenSource();

        var answerResultUtc = DateTime.UtcNow.AddSeconds(
            match.Profile.AnswerRevealDelaySeconds);

        _ = RunPhaseTimerAsync(
            match.MatchId,
            answerResultUtc,
            match.PhaseTimerCts.Token);
    }

    private void StartCaptainQuestionDelayLocked(
        VsMatchSession match)
    {
        match.PhaseTimerCts.Cancel();
        match.PhaseTimerCts.Dispose();
        match.PhaseTimerCts = new CancellationTokenSource();

        _ = RunCaptainQuestionStartAsync(
            match.MatchId,
            TimeSpan.FromSeconds(
                match.Profile.AnswerRevealDelaySeconds),
            match.PhaseTimerCts.Token);
    }

    private async Task RunCaptainQuestionStartAsync(
        Guid matchId,
        TimeSpan delay,
        CancellationToken ct)
    {
        try
        {
            await Task.Delay(delay, ct);

            if (!_store.TryGet(matchId, out var match) ||
                match is null)
            {
                return;
            }

            (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

            lock (match.SyncRoot)
            {
                if (ct.IsCancellationRequested ||
                    match.IsClosed ||
                    match.Phase !=
                        VsMatchPhase.CaptainQuestionSelection ||
                    !match.Game.SelectedCaptainLoadoutPosition.HasValue)
                {
                    return;
                }

                VsMatchGameRules
                    .BeginSelectedCaptainQuestion(match);
                StartPhaseLocked(
                    match,
                    VsMatchPhase.CaptainQuestion);

                messages =
                    VsMatchSnapshotBuilder.BuildMessages(match);
            }

            await SendBroadcastMessagesAsync(messages);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "VS captain question start failed. matchId={MatchId}",
                matchId);
        }
    }

    private void ContinueAfterQuestionResultLocked(
        VsMatchSession match)
    {
        if (match.Game.QuestionKind ==  VsQuestionKind.Guess)
        {
            VsMatchGameRules.BeginNormalQuestion(match);
            StartPhaseLocked(
                match,
                VsMatchPhase.NormalRoundQuestion);
            return;
        }

        var isCaptainRound = match.Game.CurrentRoundNumber >  match.Classification.RequiredPartySize;

        if (!isCaptainRound &&
            VsMatchGameRules.HasNextNormalQuestion(match))
        {
            VsMatchGameRules.MoveToNextNormalQuestion(match);
            StartPhaseLocked(
                match,
                VsMatchPhase.NormalRoundQuestion);
            return;
        }

        if (!isCaptainRound)
        {
            VsMatchGameRules.BuildNormalRoundResult(match);
            StartPhaseLocked(match, VsMatchPhase.NormalRoundResult);
            return;
        }

        if (VsMatchGameRules.HasNextCaptainQuestion(match))
        {
            VsMatchGameRules .MoveToNextCaptainSelection(match);

            StartPhaseLocked(  match, VsMatchPhase.CaptainQuestionSelection);
            return;
        }

        VsMatchGameRules.BuildCaptainRoundResult(match);
        StartPhaseLocked(
            match,
            VsMatchPhase.CaptainRoundResult);
    }

    private void ContinueAfterNormalRoundLocked( VsMatchSession match)
    {
        VsMatchGameRules.CommitRoundResult(match);

        if (VsMatchGameRules.HasNextNormalRound(match))
        {
            VsMatchGameRules.BeginNextNormalRound(match);
            StartPhaseLocked(
                match,
                VsMatchPhase.NormalRoundGuess);
            return;
        }

        VsMatchGameRules.BeginCaptainRound(match);
        StartPhaseLocked(
            match,
            VsMatchPhase.CaptainQuestionSelection);
    }

    private static int ResolvePhaseDuration(
        VsMatchSession match,
        VsMatchPhase phase) =>
        phase switch
        {
            VsMatchPhase.PreparationOrder or
            VsMatchPhase.PreparationCategories or
            VsMatchPhase.PreparationHelps =>
                match.Profile.PreparationSeconds,

            VsMatchPhase.PreparationStarting or
            VsMatchPhase.GameStarting =>
                match.Profile.PhasePauseSeconds,

            VsMatchPhase.NormalRoundGuess =>
                match.Profile.GuessSeconds,

            VsMatchPhase.NormalRoundQuestion or
            VsMatchPhase.CaptainQuestion =>
                match.Profile.QuestionSeconds,

            VsMatchPhase.QuestionResult =>
                match.Profile.QuestionPauseSeconds,

            VsMatchPhase.NormalRoundResult or
            VsMatchPhase.CaptainRoundResult =>
                match.Profile.RoundResultSeconds,

            VsMatchPhase.CaptainQuestionSelection =>
                match.Profile.CaptainSelectionSeconds,

            _ => 0
        };

    private static bool IsPreparationPhase(
        VsMatchPhase phase) =>
        phase is
            VsMatchPhase.PreparationOrder or
            VsMatchPhase.PreparationCategories or
            VsMatchPhase.PreparationHelps;

    private static bool IsAnswerPhase(
        VsMatchPhase phase) =>
        phase is
            VsMatchPhase.NormalRoundGuess or
            VsMatchPhase.NormalRoundQuestion or
            VsMatchPhase.CaptainQuestion;
}
