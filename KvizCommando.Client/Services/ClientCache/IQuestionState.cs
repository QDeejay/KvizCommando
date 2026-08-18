using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.ClientCache
{
    public interface IQuestionState
    {
        bool IsLoaded { get; }
        QuestionDtos? Snapshot { get; }

        int[]? FactorySlots { get; }
        UserSlot[]? Userlots { get; }
        PendingSlot[]? PendingSlots { get; }
        QuestionExtendedInfo? ExtendedInfo { get; }
      

        /// <summary>
        /// Szükség esetén betölti a képernyő aktuális állapotát.
        /// </summary>
        Task EnsureLoadedAsync();
        /// <summary>
        /// Friss adatot tölt a képernyő gyorsítótárába.
        /// </summary>
        Task RefreshAsync();
        /// <summary>
        /// Eltávolítja a cache-ből a megadott UserId-hez tartozó PlayerId-t (pl. kijelentkezéskor).
        /// </summary>
        void Invalidate();
        /// <summary>
        /// Teljes cache ürítése (pl. admin flush vagy maintenance során).
        /// </summary>
        void Clear();
    }
}
