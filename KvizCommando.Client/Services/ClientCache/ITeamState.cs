using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Services.ClientCache
{
    public interface ITeamState
    {
        bool IsLoaded { get; }
        TeamDtos? Snapshot { get; }
        TeamMemberDto[]? TeamMembers { get; }
        CandidateDto[]? Candidates { get; }
        bool[]? Charmask { get; }
        TeamExtendedInfo? TeamInfo { get; }
        HelpDto? Help { get; }
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
