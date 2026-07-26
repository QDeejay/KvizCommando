using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Services;

internal interface ISeasonProgressService
{
    void AdvanceAfterRound(GameState state);
}
