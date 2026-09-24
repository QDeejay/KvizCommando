using KvizCommando.Client.Services.ScreenData;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.ClientCache;

/// <summary>Betölti és érvényteleníti a négy ranglista közös kliensállapotát.</summary>
public sealed class RankingState : IRankingState
{
    private readonly ICacheApiService _api;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private RankingDtos? _snapshot;
    private bool _dirty = true;

    /// <summary>Létrehozza a ranglisták kliensoldali állapotát.</summary>
    /// <param name="api">A képernyőadatokat lekérő szolgáltatás.</param>
    public RankingState(ICacheApiService api)
    {
        _api = api;
    }

    /// <inheritdoc />
    public bool IsLoaded => _snapshot is not null && !_dirty;

    /// <inheritdoc />
    public RankingDtos? Snapshot => _snapshot;

    /// <inheritdoc />
    public async Task EnsureLoadedAsync()
    {
        if (IsCurrent())
            return;

        await _gate.WaitAsync();
        try
        {
            if (IsCurrent())
                return;

            _snapshot = await _api.GetRankingsAsync();
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
            _snapshot = await _api.GetRankingsAsync();
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

    private bool IsCurrent() =>
        IsLoaded &&
        DateTime.UtcNow < _snapshot!.LastFlushUtc.AddSeconds(
            _snapshot.FlushIntervalSeconds);
}
