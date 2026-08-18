using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Client.Features.VsGame.Services;

public interface IVsMatchClientService : IAsyncDisposable
{
    event Action? OnChanged;

    VsRankedQueueSnapshot? QueueSnapshot { get; }
    VsMatchSnapshot? MatchSnapshot { get; }
    VsConnectionCheckResult? ConnectionCheck { get; }
    string ErrorMessageKey { get; }
    bool IsConnected { get; }
    DateTime ServerUtcNow { get; }

    Task<VsQueueJoinResult> StartAsync(
        int classificationId,
        CancellationToken ct = default);

    Task<VsQueueLeaveStatus> LeaveQueueAsync(
        CancellationToken ct = default);

    Task SelectCharacterAsync(
        int slotNumber,
        CancellationToken ct = default);

    Task AssignLoadoutAsync(
        VsLoadoutAssignmentRequest request,
        CancellationToken ct = default);

    Task AssignHelpAsync(
        VsHelpAssignmentRequest request,
        CancellationToken ct = default);

    Task ResetPreparationAsync(CancellationToken ct = default);
    Task FinishPreparationAsync(CancellationToken ct = default);

    Task SubmitGuessAsync(
        VsGuessAnswerRequest request,
        CancellationToken ct = default);

    Task SubmitChoiceAsync(
        VsChoiceAnswerRequest request,
        CancellationToken ct = default);

    Task UseHelpAsync(
        VsUseHelpRequest request,
        CancellationToken ct = default);

    Task SelectCaptainQuestionAsync(
        VsCaptainQuestionRequest request,
        CancellationToken ct = default);

    Task StopAsync(CancellationToken ct = default);
}
