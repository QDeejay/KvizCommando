namespace KvizCommando.Server.Services.PlayerCache;

public sealed record ReportedQuestionBatch(int[] FactoryIds, int[] GuessIds, int[] UserIds)
{
    public static ReportedQuestionBatch Empty { get; } = new([], [], []);
}
