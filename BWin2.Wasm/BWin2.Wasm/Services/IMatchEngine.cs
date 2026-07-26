using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Services;

internal interface IMatchEngine
{
    Task<RoundPlayResult> PlayRoundAsync(
        GameState state,
        bool showBetMatches,
        CancellationToken ct = default);
}
