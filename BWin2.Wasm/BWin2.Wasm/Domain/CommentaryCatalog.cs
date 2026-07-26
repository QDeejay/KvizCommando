namespace BWin2.Wasm.Domain;

internal sealed class CommentaryCatalog
{
    private readonly IReadOnlyList<string> _phrases;
    private readonly IReadOnlyList<string> _scripts;

    public CommentaryCatalog(
        IReadOnlyList<string> phrases,
        IReadOnlyList<string> scripts)
    {
        _phrases = phrases;
        _scripts = scripts;
    }

    public string GetPhrase(int letterCode)
    {
        if (letterCode < 1 || letterCode > _phrases.Count)
            throw new InvalidDataException($"Unknown commentary phrase code: {letterCode}.");

        return _phrases[letterCode - 1];
    }

    /// <summary>
    /// The legacy program intentionally addresses scripts by commentCode + 25.
    /// Therefore script index zero is retained but unused by normal match events.
    /// </summary>
    public string GetScript(int commentCode)
    {
        if (commentCode < 0 || commentCode >= _scripts.Count)
            throw new InvalidDataException($"Unknown commentary script: {commentCode}.");

        return _scripts[commentCode];
    }
}
