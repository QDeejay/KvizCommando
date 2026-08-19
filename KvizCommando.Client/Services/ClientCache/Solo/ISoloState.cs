using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.ClientCache
{
    public interface ISoloState
    {
        bool IsLoaded { get; }
        SoloGameDtos? Snapshot { get; }

        public SoloEnables? Enables { get; }
        public SoloResults? REsults { get; }


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
    }
}
