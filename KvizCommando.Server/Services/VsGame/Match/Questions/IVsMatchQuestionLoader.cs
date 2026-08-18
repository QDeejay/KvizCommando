namespace KvizCommando.Server.Services.VsGame.Match;

public interface IVsMatchQuestionLoader
{
    /// <summary>
    /// A résztvevők kérdéskészleteiből összeállítja a meccs teljes kérdés- és kategóriakiosztását.
    /// </summary>
    /// <param name="players">A meccs résztvevőinek betöltéshez szükséges adatai.</param>
    /// <param name="normalRoundCount">A kapitányi kört megelőző normál körök száma.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task<VsMatchQuestionSet> LoadAsync(
        IReadOnlyCollection<VsMatchPlayerSeed> players,
        int normalRoundCount,
        CancellationToken ct = default);
}

public sealed class VsMatchQuestionSet
{
    public IReadOnlyDictionary<int, VsMatchLoadoutItemState[]> Loadouts
        { get; init; } =
        new Dictionary<int, VsMatchLoadoutItemState[]>();

    public VsMatchGuessQuestionState[] GuessQuestions { get; init; } = [];
}
