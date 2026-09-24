using KvizCommando.Client.Services.ScreenData;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.ClientCache;

/// <summary>Betölti és frissíti a négy ranglista közös kliensállapotát.</summary>
public sealed class RankingState : IRankingState
{
    private readonly ICacheApiService _api;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private RankingDtos? _snapshot;

    /// <summary>Létrehozza a ranglisták kliensoldali állapotát.</summary>
    /// <param name="api">A képernyőadatokat lekérő szolgáltatás.</param>
    public RankingState(ICacheApiService api)
    {
        _api = api;
    }

    /// <inheritdoc />
    public bool IsLoaded => _snapshot is not null;

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
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        _snapshot = null;
    }

    private bool IsCurrent() =>
        IsLoaded &&
        (_snapshot!.NextRefreshUtc is null ||
         DateTime.UtcNow < _snapshot.NextRefreshUtc.Value);
}
