using KvizCommando.Shared.Contracts.SoloGame;

namespace KvizCommando.Server.Services.SoloGame.GameCache;

public enum SoloGameStatus { Active, Finishing, Completed, Abandoned }

public sealed class CachedSoloQuestion
{
    public required int QuestionId { get; init; }
    public required string Question { get; init; }
    public required string[] Answers { get; init; }
    public required int CorrectOptionIndex { get; init; }
}

public sealed class SoloGameSession
{
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
    public SoloGameStatus Status { get; set; } = SoloGameStatus.Active;
    public SemaphoreSlim Lock { get; } = new(1, 1);
}
