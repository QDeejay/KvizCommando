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
        Task EnsureLoadedAsync();    // első használatkor betölti, ha még nem
        /// <summary>
        /// Friss adatot tölt a képernyő gyorsítótárába.
        /// </summary>
        Task RefreshAsync();         // szerver művelet után explicit frissítés
        /// <summary>
        /// Eltávolítja a cache-ből a megadott UserId-hez tartozó PlayerId-t (pl. kijelentkezéskor).
        /// </summary>
        void Invalidate();           // jelöld “piszkosnak”, következő Ensure újratölt

        /// <summary>
        /// Teljes cache ürítése (pl. admin flush vagy maintenance során).
        /// </summary>
        void Clear();                // törli a cache-t, következő Ensure újratölt

        // közvetlen elérés a 3 fő részhez
        UserMainData? UserMainData { get; }
        HomeScreen? HomeScreen { get; }
        HomeExtendedInfo? ExtendedInfo { get; }
        HomeDTOs? Snapshot { get; }
    }
}
