using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading;

namespace KvizCommando.Server.Services.PlayerCache
{
    /// <summary>
    /// Egy cache-bejegyzés egyetlen játékoshoz.
    /// Tartalmazza az adatokat, a lockot, és a metaadatokat.
    /// </summary>
    public sealed class CacheEntry
    {
        public CacheEntry(CachedPlayer player)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Lock = new SemaphoreSlim(1, 1);
            LastAccessUtc = DateTime.UtcNow;
            Dirty = DirtyFlags.None;
            CachedQ = new CachedQuestion();
        }

        /// <summary>
        /// A játékos állapota (domain adatok).
        /// </summary>
        public CachedPlayer Player { get; set; }

        /// <summary>
        /// Lock az adott játékoshoz tartozó műveletek sorba rendezésére.
        /// </summary>
        public SemaphoreSlim Lock { get; }

        /// <summary>
        /// Utolsó szerveroldali aktivitás időpontja.
        /// </summary>
        public DateTime LastAccessUtc { get; set; }

        /// <summary>
        /// Dirty bitek azonosítják, mely szegmensek módosultak.
        /// </summary>
        public DirtyFlags Dirty { get; set; }
        public CachedQuestion CachedQ { get; }
       
        public bool HasAnyDirty =>
            Dirty != DirtyFlags.None ||
            (CachedQ?.DirtyMask ?? 0) != 0;
    }
    public sealed class CachedQuestion
    {
        public UserQuestion[] uSlots { get; } = new UserQuestion[10];
        public List<FactoryQuestion> fSlots { get; set; } = new(); // nem kellő user kérdés megy majd a gyári táblába
        public PendingQuestion[] pSlots { get; } = new PendingQuestion[5];
        public uint DirtyMask { get; set; }
    }
   
}
