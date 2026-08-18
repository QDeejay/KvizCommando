using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.User;


namespace KvizCommando.Client.Services.ClientCache
{
    public interface IHomeState
    {
        bool IsLoaded { get; }
        Task EnsureLoadedAsync();    // első használatkor betölti, ha még nem
        Task RefreshAsync();         // szerver művelet után explicit frissítés
        void Invalidate();           // jelöld “piszkosnak”, következő Ensure újratölt

        void Clear();                // törli a cache-t, következő Ensure újratölt

        // közvetlen elérés a 3 fő részhez
        UserMainData? UserMainData { get; }
        HomeScreen? HomeScreen { get; }
        HomeExtendedInfo? ExtendedInfo { get; }
        HomeDTOs? Snapshot { get; }
    }
}
