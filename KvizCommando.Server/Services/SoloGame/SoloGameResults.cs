using KvizCommando.Shared.Contracts.SoloGame;

namespace KvizCommando.Server.Services.SoloGame;

public enum SoloGameOperationStatus
{
    Success,
    Rejected,
    SessionMismatch
}

public sealed class SoloStartResult
{
    public SoloGameOperationStatus Status { get; init; }
    public StartSoloGameResponse? Response { get; init; }
}

public sealed class SoloAnswerResult
{
    public SoloGameOperationStatus Status { get; init; }
    public FinishSoloGameResponse? Response { get; init; }
}
