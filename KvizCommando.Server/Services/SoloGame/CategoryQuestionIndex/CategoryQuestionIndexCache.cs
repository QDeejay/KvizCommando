using KvizCommando.Server.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;

public sealed class CategoryQuestionIndexCache : ICategoryQuestionIndexCache
{
    private const int GUESS_CATEGORY_NO = 99;
    private readonly IServiceScopeFactory _scopeFactory;

    private IReadOnlyDictionary<int, IReadOnlyList<int>> _index =
        new Dictionary<int, IReadOnlyList<int>>();

    private bool _invalidated = true;

    public CategoryQuestionIndexCache(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Betölti a szolgáltatás működéséhez szükséges adatokat.
    /// </summary>
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

        var guessQuestionIds = await db.GuessQuestions
            .AsNoTracking()
            .OrderBy(question => question.Id)
            .Select(question => question.Id)
            .ToArrayAsync(ct);

        var newIndex = rows
            .GroupBy(question => question.CategoryNo)
            .OrderBy(group => group.Key)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<int>)group
                    .Select(question => question.Id)
                    .OrderBy(id => id)
                    .ToArray());

        newIndex[GUESS_CATEGORY_NO] = guessQuestionIds;

        _index = newIndex;
        _invalidated = false;

        LogLoadedIndex();
    }

    /// <summary>
    /// Visszaadja a megadott kategóriához indexelt kérdésazonosítókat.
    /// </summary>
    public IReadOnlyList<int> GetQuestionIds(int categoryNo)
    {
        return _index.TryGetValue(categoryNo, out var questionIds)
            ? questionIds
            : Array.Empty<int>();
    }

    /// <summary>
    /// Érvényteleníti a gyorsítótárat, hogy a következő lekérés friss adatot töltsön.
    /// </summary>
    public void Invalidate()
    {
        _invalidated = true;
    }

    /// <summary>
    /// Érvénytelenítés után újratölti a kategória-kérdésindexet.
    /// </summary>
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
