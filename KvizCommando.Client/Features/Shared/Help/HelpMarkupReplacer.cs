namespace KvizCommando.Client.Features.Shared.Help;

public static class HelpMarkupReplacer
{
    /// <summary>
    /// Behelyettesíti a súgószöveg dinamikus helyőrzőit.
    /// </summary>
    /// <param name="html">A helyőrzőket tartalmazó HTML-szöveg.</param>
    /// <param name="tokens">A HTML helyőrzőihez rendelt értékek.</param>
    public static string Replace(
        string html,
        IReadOnlyDictionary<string, string> tokens)
    {
        foreach (var (name, value) in tokens)
        {
            html = html.Replace(
                $"{{{{{name}}}}}",
                value,
                StringComparison.Ordinal);
        }

        return html;
    }
}
