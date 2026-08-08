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
            ],
            [102] =
            [
                "question/user-questions-01.html",
                "question/user-questions-02.html"
            ],
            [103] =
            [
                "question/pending-questions-01.html",
                "question/pending-questions-02.html"
            ],
            [104] =
            [
                "question/new-question-01.html",
                "question/new-question-02.html"
            ]
        };
}
