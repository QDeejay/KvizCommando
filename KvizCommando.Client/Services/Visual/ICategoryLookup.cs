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
        /// <param name="code">A lokalizálandó kategóriakód.</param>
        /// <param name="culture">A kért kultúra neve, például <c>hu-HU</c>.</param>
        /// <returns>A kategóriakód lokalizált felirata; ismeretlen kódnál annak szöveges alakja.</returns>
        string ResolveLabel(int code, string culture);                 // code -> szöveg
        /// <summary>
        /// Megkísérli feloldani a kategóriakód lokalizált feliratát.
        /// </summary>
        /// <param name="code">A lokalizálandó kategóriakód.</param>
        /// <param name="label">Sikeres feloldáskor a lokalizált kategórianév.</param>
        /// <param name="culture">A kért kultúra neve, például <c>hu-HU</c>.</param>
        /// <returns><see langword="true"/>, ha a kódhoz tartozik lokalizált felirat; egyébként <see langword="false"/>.</returns>
        bool TryResolveLabel(int code, out string label, string culture);
    }
}
