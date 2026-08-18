using KvizCommando.Client.Services.Dto;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.ClientCache;

public sealed class VsState : IVsState
{
    private readonly ICacheApiService _api;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private VsGameDtos? _snapshot;
    private bool _dirty = true;

    public VsState(ICacheApiService api)
    {
        _api = api;
    }

    public bool IsLoaded => _snapshot is not null && !_dirty;
    public VsGameDtos? Snapshot => _snapshot;

    /// <summary>
    /// Szükség esetén betölti a képernyő aktuális állapotát.
    /// </summary>
    public async Task EnsureLoadedAsync()
    {
        if (IsLoaded)
            return;

        await _gate.WaitAsync();
        try
        {
            if (IsLoaded)
                return;

            _snapshot = await _api.GetVsGameAsync();
            _dirty = false;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Friss adatot tölt a képernyő gyorsítótárába.
    /// </summary>
    public async Task RefreshAsync()
    {
        await _gate.WaitAsync();
        try
        {
            _snapshot = await _api.GetVsGameAsync();
            _dirty = false;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Érvényteleníti a gyorsítótárat, hogy a következő lekérés friss adatot töltsön.
    /// </summary>
    public void Invalidate() => _dirty = true;

    /// <summary>
    /// Törli a szolgáltatásban tárolt aktuális állapotot.
    /// </summary>
    public void Clear()
    {
        _snapshot = null;
        _dirty = true;
    }
}
