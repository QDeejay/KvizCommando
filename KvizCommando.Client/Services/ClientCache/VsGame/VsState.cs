using KvizCommando.Client.Services.ScreenData;
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Invalidate() => _dirty = true;

    /// <inheritdoc />
    public void Clear()
    {
        _snapshot = null;
        _dirty = true;
    }
}
