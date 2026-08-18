using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed partial class VsMatchService
{
    public async Task SubmitGuessAsync(
        string connectionId,
        VsGuessAnswerRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);
            var receivedUtc = DateTime.UtcNow;

            if (!VsMatchGameRules.SubmitGuess(
                    match,
                    player,
                    request,
                    receivedUtc))
            {
                return;
            }

            AddLog(
                match,
                player!.PlayerId,
                "GuessSubmitted",
                $"Question={request.QuestionNumber}");

            if (VsMatchGameRules
                .HaveAllParticipantsAnswered(match))
            {
                StartAnswerResultDelayLocked(match);
            }

            messages =
                VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task SubmitChoiceAsync(
        string connectionId,
        VsChoiceAnswerRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);
            var receivedUtc = DateTime.UtcNow;

            if (!VsMatchGameRules.SubmitChoice(
                    match,
                    player,
                    request,
                    receivedUtc))
            {
                return;
            }

            AddLog(
                match,
                player!.PlayerId,
                "ChoiceSubmitted",
                $"Question={request.QuestionNumber};" +
                $"Answer={request.AnswerIndex}");

            if (VsMatchGameRules
                .HaveAllParticipantsAnswered(match))
            {
                StartAnswerResultDelayLocked(match);
            }

            messages =
                VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task UseHelpAsync(
        string connectionId,
        VsUseHelpRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);
            var receivedUtc = DateTime.UtcNow;

            if (!VsMatchGameRules.UseHelp(
                    match,
                    player,
                    request,
                    receivedUtc))
            {
                return;
            }

            AddLog(
                match,
                player!.PlayerId,
                "HelpUsed",
                $"Question={request.QuestionNumber};" +
                $"Help={player.ActiveQuestionHelp}");

            messages =
                VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }

    public async Task SelectCaptainQuestionAsync(
        string connectionId,
        VsCaptainQuestionRequest request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_store.TryGetByConnection(connectionId, out var match) ||
            match is null)
        {
            return;
        }

        (string ConnectionId, VsMatchSnapshot Snapshot)[] messages;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);
            var receivedUtc = DateTime.UtcNow;

            if (!VsMatchGameRules.SelectCaptainQuestion(
                    match,
                    player,
                    request,
                    receivedUtc))
            {
                return;
            }

            AddLog(
                match,
                player!.PlayerId,
                "CaptainQuestionSelected",
                $"Position={request.LoadoutPosition}");

            StartPhaseLocked(
                match,
                VsMatchPhase.CaptainQuestion);

            messages =
                VsMatchSnapshotBuilder.BuildMessages(match);
        }

        await SendBroadcastMessagesAsync(messages);
    }
}
