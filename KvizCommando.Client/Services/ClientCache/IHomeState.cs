using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.User;


namespace KvizCommando.Client.Services.ClientCache
{
    public interface IHomeState
    {
        bool IsLoaded { get; }
        /// <summary>
        /// Szükség esetén betölti a képernyő aktuális állapotát.
        /// </summary>
        Task EnsureLoadedAsync();
        /// <summary>
        /// Friss adatot tölt a képernyő gyorsítótárába.
        /// </summary>
        Task RefreshAsync();
        /// <summary>
        /// Elavultnak jelöli az állapotpillanatképet, de az utolsó betöltött adatot megtartja a következő frissítésig.
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Törli az állapotpillanatképet; a következő betöltési kérés ismét a szerverről tölti fel.
        /// </summary>
        void Clear();

        UserMainData? UserMainData { get; }
        HomeScreen? HomeScreen { get; }
        HomeExtendedInfo? ExtendedInfo { get; }
        HomeDTOs? Snapshot { get; }
    }
}
