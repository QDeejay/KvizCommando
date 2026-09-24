using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.ClientCache;

/// <summary>A ranglisták közös kliensoldali pillanatképét kezeli.</summary>
public interface IRankingState
{
    /// <summary>Jelzi, hogy a pillanatkép be van-e töltve.</summary>
    bool IsLoaded { get; }

    /// <summary>A négy ranglista legutóbb lekért adatai.</summary>
    RankingDtos? Snapshot { get; }

    /// <summary>
    /// Szükség esetén betölti a ranglisták aktuális állapotát.
    /// </summary>
    Task EnsureLoadedAsync();

    /// <summary>
    /// Friss adatot tölt a ranglisták gyorsítótárába.
    /// </summary>
    Task RefreshAsync();

    /// <summary>
    /// Elavultnak jelöli a ranglisták pillanatképét.
    /// </summary>
    void Invalidate();

    /// <summary>
    /// Törli a ranglisták pillanatképét.
    /// </summary>
    void Clear();
}
