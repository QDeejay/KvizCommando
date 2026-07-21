namespace KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;

public interface ICategoryQuestionIndexCache
{
    Task<IReadOnlyList<int>> GetQuestionIdsAsync(
        int categoryNo,
        CancellationToken ct = default);

    void Invalidate();
}
