namespace BWin2.Wasm.State;

internal enum MatchPresentationPhase
{
    Hidden,
    Introduction,
    Live,
    Penalties,
    Finished
}

internal sealed record MatchGoalVm(
    string Player,
    string Team,
    int Minute,
    int HomeScore,
    int AwayScore);
