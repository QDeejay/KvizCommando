namespace BWin2.Wasm.Domain;

internal sealed record ScorerEntry(
    string PlayerName,
    string TeamShortName,
    int Goals,
    bool IsSelectedTeam);
