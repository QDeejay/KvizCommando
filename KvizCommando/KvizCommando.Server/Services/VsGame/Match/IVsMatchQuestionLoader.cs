namespace KvizCommando.Server.Services.VsGame.Match;

public interface IVsMatchQuestionLoader
{
    Task<IReadOnlyDictionary<int, VsMatchLoadoutItemState[]>> LoadAsync(
        IReadOnlyCollection<VsMatchPlayerSeed> players,
        int loadoutSize,
        CancellationToken ct = default);
}

/**
 * A MatchLocked után egyszer végrehajtott, kötegelt
 * kérdésbetöltés szerződése.
 */
