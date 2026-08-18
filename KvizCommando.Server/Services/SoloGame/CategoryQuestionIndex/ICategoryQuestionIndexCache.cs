namespace KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;

public interface ICategoryQuestionIndexCache
{
    /// <summary>
    /// Betölti a kérdésazonosítókat tartalmazó kategóriaindexet az adatbázisból.
    /// </summary>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task LoadAsync(CancellationToken ct = default);

    /// <summary>
    /// Visszaadja a megadott kategóriához indexelt kérdésazonosítókat.
    /// </summary>
    /// <param name="categoryNo">A kérdéskategória egytől induló azonosítója.</param>
    /// <returns>A kategóriához tartozó kérdésazonosítók csak olvasható listája.</returns>
    IReadOnlyList<int> GetQuestionIds(int categoryNo);

    /// <summary>
    /// Érvénytelennek jelöli a teljes kategóriaindexet; a következő újratöltés minden kategóriát frissít.
    /// </summary>
    void Invalidate();

    /// <summary>
    /// Érvénytelenítés után újratölti a kategória-kérdésindexet.
    /// </summary>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task ReloadIfInvalidatedAsync(CancellationToken ct = default);
}
