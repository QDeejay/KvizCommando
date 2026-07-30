namespace KvizCommando.Server.Services.VsGame.Match;

public interface IVsMatchQuestionLoader
{
    Task<VsMatchQuestionSet> LoadAsync(
        IReadOnlyCollection<VsMatchPlayerSeed> players,
        int loadoutSize,
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

/**
 * MÓDOSÍTÁS: a meccs loadoutkérdései mellett ugyanabban az egyszeri
 * betöltésben adja vissza a normál körök tippkérdéseit.
 *
 * A MatchLocked után egyszer végrehajtott, kötegelt kérdésbetöltés
 * szerződése.
 */
