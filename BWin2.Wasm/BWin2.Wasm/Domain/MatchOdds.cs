namespace BWin2.Wasm.Domain;

internal readonly record struct MatchOdds(
    string HomeText,
    string DrawText,
    string AwayText)
{
    public string GetText(BetSelection selection) => selection switch
    {
        BetSelection.Home => AwayText,
        BetSelection.Draw => DrawText,
        BetSelection.Away => HomeText,
        _ => string.Empty
    };
}
