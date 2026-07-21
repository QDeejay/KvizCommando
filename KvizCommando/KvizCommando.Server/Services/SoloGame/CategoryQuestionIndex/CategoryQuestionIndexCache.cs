using KvizCommando.Server.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;

public sealed class CategoryQuestionIndexCache : ICategoryQuestionIndexCache
{
    private readonly IServiceScopeFactory _scopeFactory;

    private IReadOnlyDictionary<int, IReadOnlyList<int>> _index =
        new Dictionary<int, IReadOnlyList<int>>();

    private bool _invalidated = true;

    public CategoryQuestionIndexCache(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        var rows = await db.FactoryQuestions
            .AsNoTracking()
            .Where(question => question.CategoryNo > 0)
            .Select(question => new
            {
                question.CategoryNo,
                question.Id
            })
            .ToListAsync(ct);

        var newIndex = rows
            .GroupBy(question => question.CategoryNo)
            .OrderBy(group => group.Key)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<int>)group
                    .Select(question => question.Id)
                    .OrderBy(id => id)
                    .ToArray());

        _index = newIndex;
        _invalidated = false;

        LogLoadedIndex();
    }

    public IReadOnlyList<int> GetQuestionIds(int categoryNo)
    {
        return _index.TryGetValue(categoryNo, out var questionIds)
            ? questionIds
            : Array.Empty<int>();
    }

    public void Invalidate()
    {
        _invalidated = true;
    }

    public async Task ReloadIfInvalidatedAsync(CancellationToken ct = default)
    {
        if (!_invalidated)
            return;

        await LoadAsync(ct);
    }

    private void LogLoadedIndex()
    {
        foreach (var category in _index.OrderBy(item => item.Key))
        {
            Console.WriteLine(
                $"[CategoryQuestionIndexCache] Kategória {category.Key}: " +
                $"{category.Value.Count} kérdésindex betöltve.");
        }

        Console.WriteLine(
            $"[CategoryQuestionIndexCache] Összesen: " +
            $"{_index.Sum(item => item.Value.Count)} kérdésindex, " +
            $"{_index.Count} kategória.");
    }
}
