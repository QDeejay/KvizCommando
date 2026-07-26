namespace BWin2.Wasm.Domain;

internal sealed record PlayedMatch(
    Fixture Fixture,
    int HomeScore,
    int AwayScore,
    bool ExtraTime,
    bool Penalties,
    int PenaltyHomeScore,
    int PenaltyAwayScore,
    string ResultText);

internal sealed record RoundPlayResult(
    IReadOnlyList<PlayedMatch> Matches,
    bool HadVisibleMatch);
