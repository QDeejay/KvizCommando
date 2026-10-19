using KvizCommando.Shared.Contracts.SoloGame;

namespace KvizCommando.Server.Services.SoloGame.GameCache;

public sealed class CachedSoloQuestion
{
    public required int QuestionId { get; init; }
    public required string Question { get; init; }
    public required string[] Answers { get; init; }
    public required int CorrectOptionIndex { get; init; }
}

public sealed class SoloGameSession
{
    public required string ConnectionId { get; init; }
    public SoloGamePhase Phase { get; set; } = SoloGamePhase.Preparing;
    public int QuestionIndex { get; set; }
    public long PhaseStartedTimestamp { get; set; }
    public DateTime? DeadlineUtc { get; set; }
    public TaskCompletionSource QuestionCompletion { get; set; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    public CancellationTokenSource LifetimeCts { get; } = new();
    public Task? RunTask { get; set; }
    public int[] ReportedQuestionIndexes { get; set; } = [];
    public required Guid GameId { get; init; }
    public required int PlayerId { get; init; }
    public required string SessionId { get; init; }
    public required SoloGameMode Mode { get; init; }
    public required int SelectionId { get; init; }
    public required int Level { get; init; }
    public bool IsHealing { get; init; }
    public required int PointsPerLevel { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
    public required IReadOnlyList<CachedSoloQuestion> Questions { get; init; }
    public List<SoloAnswerDto> Answers { get; } = [];
    public SemaphoreSlim Lock { get; } = new(1, 1);
}
