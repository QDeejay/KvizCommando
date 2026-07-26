using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.ClientCache;

public interface IVsState
{
    bool IsLoaded { get; }
    VsGameDtos? Snapshot { get; }

    Task EnsureLoadedAsync();
    Task RefreshAsync();
    void Invalidate();
    void Clear();
}
