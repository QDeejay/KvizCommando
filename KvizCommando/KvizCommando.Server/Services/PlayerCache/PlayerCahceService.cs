using KvizCommando.Server.Services.Db;
using System.Collections.Concurrent;


namespace KvizCommando.Server.Services.PlayerCache
{
    public sealed class PlayerCacheService : IPlayerCacheService
    {
        private readonly IPlayerDbService _playerDb;
        private readonly IQuestionDbService _questionDb;

        private static readonly ConcurrentDictionary<int, CacheEntry> _entries = new();


        public PlayerCacheService(
            IPlayerDbService playerdb,
            IQuestionDbService questiondb)
        {
            _playerDb = playerdb;
            _questionDb = questiondb;
        }

        /// <summary>
        /// Az összes aktív user azonosítója, akik jelenleg bent vannak a cache-ben.
        /// </summary>
        public IReadOnlyCollection<int> GetActivePlayerIds()
            => _entries.Keys.ToList();

        private async Task<CacheEntry?> GetOrCreateEntryAsync(
            int playerId,
            string sessionId,
            CancellationToken ct)
        {
            if (_entries.TryGetValue(playerId, out var entry))
                return entry;

            var cp = await _playerDb.LoadPlayerFromDbAsync(playerId, sessionId, ct);
            var cq = await _questionDb.LoadQuestionsFromDbAsync(playerId, ct);

            if (cp is null) return null;
            if (cq is null) return null;
            entry = new CacheEntry(cp)
            {
                Dirty = DirtyFlags.None,
                LastAccessUtc = DateTime.UtcNow
            };
            for (int i = 0; i < cq.uSlots.Length; i++)
            {
                entry.CachedQ.uSlots[i] = cq.uSlots[i];
            }
            for (int i = 0; i < cq.pSlots.Length; i++)
            {
                entry.CachedQ.pSlots[i] = cq.pSlots[i];
            }

            _entries[playerId] = entry;
            return entry;
        }

        // --------------------------------------------------------
        // GET / LOAD
        // --------------------------------------------------------
        public async Task<(CachedPlayer?, CachedQuestion?)> GetOrLoadLockedAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return (null, null);

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return (new CachedPlayer
                    {
                        SessionId = "denied"
                    }, null);

                entry.LastAccessUtc = DateTime.UtcNow;
                return (entry.Player, entry.CachedQ);
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        // --------------------------------------------------------
        // Új session check
        // --------------------------------------------------------
        public async Task<bool> NewSessionCheckLockedAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default)
        {
            if (!_entries.TryGetValue(playerId, out var entry))
            {
                ///
                var Entry = await GetOrCreateEntryAsync(playerId, sessionId, ct); /// Ideiglenes

                return false;
            }


            await entry.Lock.WaitAsync(ct);
            try
            {
                entry.Player.SessionId = sessionId;
                entry.LastAccessUtc = DateTime.UtcNow;
                return true;
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        public async Task<bool?> UpdatePlayerLockedAsync(
            int playerId,
            string sessionId,
            Func<CachedPlayer, DirtyFlags?> update,
            CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return false;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return null;

                var dirty = update(entry.Player);
                if (dirty is null)
                    return false;

                entry.Dirty |= dirty.Value;
                entry.LastAccessUtc = DateTime.UtcNow;
                return true;
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        public async Task<bool?> UpdateQuestionsLockedAsync(
            int playerId,
            string sessionId,
            Func<CachedPlayer, CachedQuestion, uint?> update,
            CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return false;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return null;

                var dirtyMask = update(entry.Player, entry.CachedQ);
                if (dirtyMask is null)
                    return false;

                entry.CachedQ.DirtyMask |= dirtyMask.Value;
                entry.LastAccessUtc = DateTime.UtcNow;
                return true;
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        // --------------------------------------------------------
        // Logout jelzés
        // --------------------------------------------------------
        public async Task<bool?> LogoutLockedRequestAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default)
        {
            if (!_entries.TryGetValue(playerId, out var entry))
                return false;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return null;

                if (entry.HasAnyDirty == false)
                {
                    _entries.TryRemove(playerId, out _);
                }
                else
                {
                    entry.Dirty |= DirtyFlags.Logout;
                    entry.LastAccessUtc = DateTime.UtcNow;
                }

                return true;
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        // --------------------------------------------------------
        // Dirty mentés DB-be
        // --------------------------------------------------------
        public async Task<(SaveResult, bool)> SaveDirtyLockedAsync(
            int playerId,
            CancellationToken ct = default)
        {
            if (!_entries.TryGetValue(playerId, out var entry))
                return (SaveResult.None, false);

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (DateTime.UtcNow - entry.LastAccessUtc > TimeSpan.FromMinutes(10) && entry.Dirty == DirtyFlags.None && entry.CachedQ.DirtyMask == 0)
                {
                    entry.Dirty = DirtyFlags.Logout;
                    return (SaveResult.Obscolated, false);
                }
                if (entry.Dirty == DirtyFlags.None)
                    return (SaveResult.None, entry.CachedQ.DirtyMask != 0);

                var saved = await _playerDb.SavePlayerToDbAsync(
                    entry.Player,
                    entry.Dirty,
                    playerId,
                    ct);

                if (saved == false)
                    return (SaveResult.None, entry.CachedQ.DirtyMask != 0);

                entry.Dirty &= ~(DirtyFlags.Core
                               | DirtyFlags.Loadout
                               | DirtyFlags.Characters
                               | DirtyFlags.AskStats
                               | DirtyFlags.CategoryStats
                               | DirtyFlags.OrientStats);

                if ((entry.Dirty & DirtyFlags.Logout) != 0 && entry.CachedQ.DirtyMask == 0)
                {
                    _entries.TryRemove(playerId, out _);
                    return (SaveResult.Logout, false);
                }

                return (SaveResult.Dirty, entry.CachedQ.DirtyMask != 0);
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        public async Task<int> SaveDirtyQuestionLockedAsync(
            int playerId,
            CancellationToken ct = default)
        {

            if (!_entries.TryGetValue(playerId, out var entry))
                return 0;

            await entry.Lock.WaitAsync(ct);

            try
            {
                if (entry.CachedQ.DirtyMask == 0)
                    return 0;
                var questionStats = await _questionDb.SaveQuestionsToDbAsync(entry.CachedQ, ct);
                entry.CachedQ.DirtyMask = 0;
                Console.WriteLine($"" +
                    $"User:{entry.Player.Core.PlayerId} " +
                    $"Saved: Usr: {questionStats.userQuestions} " +
                    $"Pnd: {questionStats.pendingQuestions} " +
                    $"Trs:{questionStats.transferedQuestions} " +
                    $"Total:{questionStats.totalQuestions}");
                if (questionStats.transferedQuestions > 0) entry.CachedQ.fSlots.Clear();
                return questionStats.totalQuestions;
            }
            catch
            {
                return 0;

            }
            finally
            {
                entry.Lock.Release();
            }
        }

    }
}
