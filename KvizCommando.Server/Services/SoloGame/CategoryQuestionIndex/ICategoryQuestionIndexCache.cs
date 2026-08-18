namespace KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;

public interface ICategoryQuestionIndexCache
{
    /// <summary>
    /// Betölti a szolgáltatás működéséhez szükséges adatokat.
    /// </summary>
    Task LoadAsync(CancellationToken ct = default);

    /// <summary>
    /// Visszaadja a megadott kategóriához indexelt kérdésazonosítókat.
    /// </summary>
    IReadOnlyList<int> GetQuestionIds(int categoryNo);

    /// <summary>
    /// Eltávolítja a cache-ből a megadott UserId-hez tartozó PlayerId-t (pl. kijelentkezéskor).
    /// </summary>
    void Invalidate();

    /// <summary>
    /// Érvénytelenítés után újratölti a kategória-kérdésindexet.
    /// </summary>
    Task ReloadIfInvalidatedAsync(CancellationToken ct = default);
}
