namespace KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;

public interface ICategoryQuestionIndexCache
{
    Task LoadAsync(CancellationToken ct = default);

    IReadOnlyList<int> GetQuestionIds(int categoryNo);

    void Invalidate();

    Task ReloadIfInvalidatedAsync(CancellationToken ct = default);
}
