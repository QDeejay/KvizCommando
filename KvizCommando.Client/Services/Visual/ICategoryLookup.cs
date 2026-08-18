namespace KvizCommando.Client.Services.Visual
{
    public interface ICategoryLookupService
    {
        /// <summary>
        /// Visszaadja az összes választható kategóriát.
        /// </summary>
        IReadOnlyList<CategoryOption> GetAll();        // 1..16
        /// <summary>
        /// Visszaadja a kategóriakód lokalizált feliratát.
        /// </summary>
        string ResolveLabel(int code, string culture);                 // code -> szöveg
        /// <summary>
        /// Megkísérli feloldani a kategóriakód lokalizált feliratát.
        /// </summary>
        bool TryResolveLabel(int code, out string label, string culture);
    }
}
