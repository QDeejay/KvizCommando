namespace KvizCommando.Client.Features.Shared.Help;

public static class HelpCollection
{
    public const string SeenStorageKey = "SeenHelps";

    public static IReadOnlyDictionary<int, string[]> Pages { get; } =
        new Dictionary<int, string[]>
        {
            [101] =
            [
                "question/loadout-01.html",
                "question/loadout-02.html",
                "question/loadout-03.html"
            ]
        };
}
