namespace KvizCommando.Server.Services.VsGame.Match;

public interface IVsMatchQuestionLoader
{
    /// <summary>
    /// Betölti a szolgáltatás működéséhez szükséges adatokat.
    /// </summary>
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
