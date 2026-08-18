namespace KvizCommando.Client.Services.Visual
{
    public interface ICategoryLookupService
    {
        IReadOnlyList<CategoryOption> GetAll();        // 1..16
        string ResolveLabel(int code, string culture);                 // code -> szöveg
        bool TryResolveLabel(int code, out string label, string culture);
    }
}
