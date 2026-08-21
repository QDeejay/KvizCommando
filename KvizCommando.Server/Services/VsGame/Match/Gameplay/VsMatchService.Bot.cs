using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed partial class VsMatchService
{
    private void ScheduleBotActionsLocked(VsMatchSession match)
    {
        if (match.Phase ==
            VsMatchPhase.CaptainQuestionSelection)
        {
            ScheduleBotCaptainQuestionLocked(match);
            return;
        }

        if (!IsAnswerPhase(match.Phase))
            return;

        foreach (var bot in match.Players.Where(player =>
                     player.IsBot &&
                     player.CurrentAnswer is null))
        {
            ScheduleBotAnswerLocked(match, bot);
        }
    }

    private void ScheduleBotCaptainQuestionLocked(
        VsMatchSession match)
    {
        if (match.Game.CaptainOrder.Length <=
            match.Game.CaptainOrderIndex)
        {
            return;
        }

        var captainPosition =
            match.Game.CaptainOrder[
                match.Game.CaptainOrderIndex];
        var captain = match.Players.First(player =>
            player.Position == captainPosition);

        if (!captain.IsBot)
            return;

        _ = RunBotCaptainQuestionAsync(
            match.MatchId,
            TimeSpan.FromSeconds(
                match.Profile.BotCaptainSelectionSeconds),
            match.PhaseTimerCts.Token);
    }

    private async Task RunBotCaptainQuestionAsync(
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
                    match.Game.SelectedCaptainLoadoutPosition.HasValue)
                {
                    return;
                }

                VsMatchGameRules
                    .SelectDefaultCaptainQuestion(match);

                var captainPosition =
                    match.Game.CaptainOrder[
                        match.Game.CaptainOrderIndex];
                var captain = match.Players.First(player =>
                    player.Position == captainPosition);

                AddLog(
                    match,
                    captain.PlayerId,
                    "BotCaptainQuestionSelected",
                    $"Position={match.Game.SelectedCaptainLoadoutPosition}");

                StartCaptainQuestionDelayLocked(match);
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
                "VS bot captain selection failed. matchId={MatchId}",
                matchId);
        }
    }

    private void ScheduleBotAnswerLocked(
        VsMatchSession match,
        VsMatchPlayerState bot)
    {
        if (!IsAnswerPhase(match.Phase) ||
            !bot.IsBot ||
            bot.CurrentAnswer is not null ||
            !VsMatchGameRules.CanAnswerCurrentQuestion(
                match,
                bot))
        {
            return;
        }

        var maximumSeconds = Math.Min(
            match.Profile.BotMaximumAnswerSeconds,
            Math.Max(1, ResolvePhaseDuration(match, match.Phase) - 1));
        var minimumSeconds = Math.Min(
            match.Profile.BotMinimumAnswerSeconds,
            maximumSeconds);
        var delaySeconds = Random.Shared.Next(
            minimumSeconds,
            maximumSeconds + 1);

        _ = RunBotAnswerAsync(
            match.MatchId,
            bot.Position,
            match.Game.QuestionNumber,
            TimeSpan.FromSeconds(delaySeconds),
            match.PhaseTimerCts.Token);
    }

    private async Task RunBotAnswerAsync(
        Guid matchId,
        int botPosition,
        int questionNumber,
        TimeSpan delay,
        CancellationToken ct)
    {
        try
        {
            await Task.Delay(delay, ct);

            if (!_store.TryGet(matchId, out var match) || match is null)
                return;

            (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

            lock (match.SyncRoot)
            {
                if (ct.IsCancellationRequested ||
                    match.IsClosed ||
                    match.Game.QuestionNumber != questionNumber)
                {
                    return;
                }

                var bot = match.Players.First(player =>
                    player.Position == botPosition);

                if (!VsMatchBotRules.SubmitAnswer(
                        match,
                        bot,
                        DateTime.UtcNow))
                {
                    return;
                }

                AddLog(
                    match,
                    bot.PlayerId,
                    "BotAnswerSubmitted",
                    $"Question={questionNumber}");

                if (VsMatchGameRules.HaveAllParticipantsAnswered(match))
                    StartAnswerResultDelayLocked(match);

                messages = VsMatchSnapshotBuilder.BuildMessages(match);
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
                "VS bot answer failed. matchId={MatchId}, position={Position}",
                matchId,
                botPosition);
        }
    }
}
