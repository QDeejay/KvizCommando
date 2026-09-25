using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed partial class VsMatchService
{
    public async Task SubmitQuestionReportsAsync(
        string connectionId,
        int[] questionNumbers,
        CancellationToken ct = default)
    {
        if (!_store.TryGetByConnection(connectionId, out var match) || match is null)
            return;

        int playerId;
        string sessionId;
        ReportedQuestionBatch reports;

        lock (match.SyncRoot)
        {
            var player = match.FindByConnection(connectionId);
            if (match.IsClosed || match.Phase != VsMatchPhase.GameCompleted ||
                player is null || !player.IsConnected || player.IsBot ||
                player.ReportsSubmitted || questionNumbers is null)
                return;

            player.ReportsSubmitted = true;
            playerId = player.PlayerId;
            sessionId = player.SessionId;

            var selected = questionNumbers.Distinct()
                .Select(number => match.ReportableQuestions.GetValueOrDefault(number))
                .Where(question => question is { Id: > 0 })
                .Select(question => question!)
                .ToArray();
            reports = new ReportedQuestionBatch(
                [.. selected.Where(question => question.Kind == VsQuestionKind.Choice &&
                                               !question.IsOwnQuestion).Select(question => question.Id)],
                [.. selected.Where(question => question.Kind == VsQuestionKind.Guess)
                            .Select(question => question.Id)],
                [.. selected.Where(question => question.Kind == VsQuestionKind.Choice &&
                                               question.IsOwnQuestion).Select(question => question.Id)]);
        }

        if (reports.FactoryIds.Length + reports.GuessIds.Length + reports.UserIds.Length == 0)
            return;

        await _rewardPersistence.SaveReportsAsync(playerId, sessionId, reports, ct);
    }
}
