using KvizCommando.Server.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;

public sealed class CategoryQuestionIndexCache : ICategoryQuestionIndexCache
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SemaphoreSlim _reloadLock = new(1, 1);

    private IReadOnlyDictionary<int, IReadOnlyList<int>> _index =
        new Dictionary<int, IReadOnlyList<int>>();

    private int _invalidated = 1;

    public CategoryQuestionIndexCache(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<IReadOnlyList<int>> GetQuestionIdsAsync(
        int categoryNo,
        CancellationToken ct = default)
    {
        if (categoryNo <= 0)
            return Array.Empty<int>();

        await EnsureLoadedAsync(ct);

        var index = Volatile.Read(ref _index);

        return index.TryGetValue(categoryNo, out var questionIds)
            ? questionIds
            : Array.Empty<int>();
    }

    public void Invalidate()
    {
        Volatile.Write(ref _invalidated, 1);
    }

    private async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (Volatile.Read(ref _invalidated) == 0)
            return;

        await _reloadLock.WaitAsync(ct);

        try
        {
            if (Volatile.Read(ref _invalidated) == 0)
                return;

            Volatile.Write(ref _invalidated, 0);

            try
            {
                var newIndex = await LoadIndexAsync(ct);
                Volatile.Write(ref _index, newIndex);

                LogLoadedIndex(newIndex);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                Volatile.Write(ref _invalidated, 1);
                throw;
            }
            catch (Exception ex)
            {
                Volatile.Write(ref _invalidated, 1);

                var currentIndex = Volatile.Read(ref _index);

                if (currentIndex.Count == 0)
                    throw;

                Console.WriteLine(
                    $"[CategoryQuestionIndexCache] Az index újratöltése sikertelen. " +
                    $"A korábbi snapshot marad használatban. Hiba: {ex.Message}");
            }
        }
        finally
        {
            _reloadLock.Release();
        }
    }

    private async Task<IReadOnlyDictionary<int, IReadOnlyList<int>>> LoadIndexAsync(
        CancellationToken ct)
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

        return rows
            .GroupBy(question => question.CategoryNo)
            .OrderBy(group => group.Key)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<int>)Array.AsReadOnly(
                    group
                        .Select(question => question.Id)
                        .OrderBy(id => id)
                        .ToArray()));
    }

    private static void LogLoadedIndex(
        IReadOnlyDictionary<int, IReadOnlyList<int>> index)
    {
        foreach (var category in index.OrderBy(item => item.Key))
        {
            Console.WriteLine(
                $"[CategoryQuestionIndexCache] Kategória {category.Key}: " +
                $"{category.Value.Count} kérdésindex betöltve.");
        }

        Console.WriteLine(
            $"[CategoryQuestionIndexCache] Összesen: " +
            $"{index.Sum(item => item.Value.Count)} kérdésindex, " +
            $"{index.Count} kategória.");
    }
}
