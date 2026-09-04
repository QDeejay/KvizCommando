namespace KvizCommando.Admin;

internal static class QuestionCategories
{
    private static readonly IReadOnlyDictionary<int, string> NAMES =
        new Dictionary<int, string>
        {
            [1] = "Vallás",
            [2] = "Dátumok és idő",
            [3] = "Zene",
            [4] = "Sport",
            [5] = "Technologia",
            [6] = "Természet tudományok",
            [7] = "Híres emberek",
            [8] = "Képzőművészet",
            [9] = "Mitologia",
            [10] = "Történelem",
            [11] = "Filmek",
            [12] = "Játék",
            [13] = "Informatika",
            [14] = "Földrajz-Csillagászat",
            [15] = "Divat és márkák",
            [16] = "Irodalom",
            [99] = "Tipp"
        };

    public static string GetName(int categoryNo) =>
        NAMES.TryGetValue(categoryNo, out var name)
            ? name
            : throw new InvalidOperationException($"Ismeretlen kategória: {categoryNo}");
}
