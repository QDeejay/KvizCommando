using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Models;

namespace KvizCommando.Server.Services.PlayerCache
{
    /// <summary>
    /// Egy játékos teljes cache-elt állapota.
    /// A szerveroldali logika számára authoritative forrás,
    /// DB-vel periodikusan szinkronizálva.
    /// </summary>
    public sealed class CachedPlayer
    {
        public Player Core { get; set; } = default!;
        public CharachterSlot?[] Characters { get; set; } = new CharachterSlot?[8];
        public RecruitSlot?[] CandidateCharacters { get; set; } = new RecruitSlot[8];
        public bool[] CharCatMask { get; set; } = new bool[8];
        //public bool CandidateChanged { get; set; } = false;

        public PlayerLoadout Loadout { get; set; } = default!;


        public PlayerAskStats AskStats { get; set; } = default!;
        public List<PlayerCategoryStat> CategoryStats { get; set; } = [];
        public List<PlayerOrientStat> OrientStats { get; set; } = [];
        /// <summary>
        /// Session ID azonosító (pl. bejelentkezés után).
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

    }

}
