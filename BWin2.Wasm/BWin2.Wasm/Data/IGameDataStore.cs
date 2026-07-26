using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Data;

internal interface IGameDataStore
{
    Task<GameState> LoadAsync(CancellationToken ct = default);
}
