using KvizCommando.Server.Services.Db;
using KvizCommando.Server.Services.VsGame;
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

        /// <inheritdoc />
        public IReadOnlyCollection<int> GetActivePlayerIds()
            => _entries.Keys.ToList();

        /// <inheritdoc />
        public bool IsNormalizedTeamNameInUse(
            string normalizedTeamName,
            int excludedPlayerId) =>
            _entries.Any(entry =>
                entry.Key != excludedPlayerId &&
                string.Equals(
                    entry.Value.Player.Core.NormalizedTeamName,
                    normalizedTeamName,
                    StringComparison.Ordinal));

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

        /// <inheritdoc />
        public async Task<CacheReadResult> GetOrLoadLockedAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null)
            {
                return new CacheReadResult
                {
                    Status = CacheReadStatus.NotFound
                };
            }

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                {
                    return new CacheReadResult
                    {
                        Status = CacheReadStatus.SessionMismatch
                    };
                }

                entry.LastAccessUtc = DateTime.UtcNow;
                return new CacheReadResult
                {
                    Status = CacheReadStatus.Success,
                    Player = entry.Player,
                    Questions = entry.CachedQ
                };
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        /// <inheritdoc />
        public async Task<CacheReadStatus> CheckSessionLockedAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default)
        {
            if (!_entries.TryGetValue(playerId, out var entry))
                return CacheReadStatus.NotFound;

            await entry.Lock.WaitAsync(ct);
            try
            {
                return entry.Player.SessionId == sessionId
                    ? CacheReadStatus.Success
                    : CacheReadStatus.SessionMismatch;
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        /// <inheritdoc />
        public async Task<bool> NewSessionCheckLockedAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default)
        {
            if (!_entries.TryGetValue(playerId, out var entry))
            {
                // A betöltés létrehozza a későbbi kérésekhez szükséges cache-bejegyzést.
                var Entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);

                return false;
            }


            await entry.Lock.WaitAsync(ct);
            try
            {
                var previousSessionReplaced = !string.Equals(
                    entry.Player.SessionId,
                    sessionId,
                    StringComparison.Ordinal);

                entry.Player.SessionId = sessionId;
                entry.LastAccessUtc = DateTime.UtcNow;
                return previousSessionReplaced;
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        /// <inheritdoc />
        public async Task<CacheUpdateResult> UpdatePlayerLockedAsync(
            int playerId,
            string sessionId,
            Func<CachedPlayer, DirtyFlags?> update,
            CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return CacheUpdateResult.NotFound;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return CacheUpdateResult.SessionMismatch;

                return ApplyPlayerUpdate(entry, update)
                    ? CacheUpdateResult.Updated
                    : CacheUpdateResult.Rejected;
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        /// <inheritdoc />
        public async Task<CacheUpdateResult> UpdatePlayerAndQuestionsLockedAsync(
            int playerId,
            string sessionId,
            Func<CachedPlayer, CachedQuestion, DirtyFlags?> update,
            CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return CacheUpdateResult.NotFound;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return CacheUpdateResult.SessionMismatch;

                return ApplyPlayerUpdate(
                        entry,
                        player => update(player, entry.CachedQ))
                    ? CacheUpdateResult.Updated
                    : CacheUpdateResult.Rejected;
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        /// <inheritdoc />
        public async Task<bool> UpdateRewardPlayerLockedAsync(
            int playerId,
            string loadSessionId,
            Func<CachedPlayer, DirtyFlags?> update,
            CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(
                playerId,
                loadSessionId,
                ct);
            if (entry is null) return false;

            await entry.Lock.WaitAsync(ct);
            try
            {
                return ApplyPlayerUpdate(entry, update);
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        private static bool ApplyPlayerUpdate(
            CacheEntry entry,
            Func<CachedPlayer, DirtyFlags?> update)
        {
            var dirty = update(entry.Player);
            if (dirty is null)
                return false;

            if ((dirty.Value & DirtyFlags.Characters) != 0 &&
                entry.Player.BattleTeamSlots.Length > 0 &&
                !HasEligibleBattleTeam(entry.Player))
            {
                Array.Clear(entry.Player.BattleTeamSlots);
            }

            entry.Dirty |= dirty.Value;
            entry.LastAccessUtc = DateTime.UtcNow;
            return true;
        }

        private static bool HasEligibleBattleTeam(CachedPlayer player)
        {
            var selectedSlots = player.BattleTeamSlots;

            if (!VsBattleClassificationRules.IsSupportedPartySize(
                    selectedSlots.Length) ||
                selectedSlots.Any(slot => slot is < 1 or > 8) ||
                selectedSlots.Distinct().Count() != selectedSlots.Length)
            {
                return false;
            }

            var selectedMembers = selectedSlots
                .Select(slot => player.Characters[slot - 1])
                .ToArray();

            if (selectedMembers.Any(member =>
                    member is null ||
                    !VsBattleClassificationRules.CanSelectMember(
                        player.Core.RankEnum,
                        member.EnergyPoints,
                        member.Rank,
                        member.XP)))
            {
                return false;
            }

            return VsBattleClassificationRules
                .GetEligibleClassificationIds(
                    player.Core.RankEnum,
                    selectedMembers
                        .Select(member => member!.Rank)
                        .ToArray())
                .Length > 0;
        }

        /// <inheritdoc />
        public async Task<CacheUpdateResult> UpdateQuestionsLockedAsync(
            int playerId,
            string sessionId,
            Func<CachedPlayer, CachedQuestion, uint?> update,
            CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return CacheUpdateResult.NotFound;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return CacheUpdateResult.SessionMismatch;

                return ApplyQuestionUpdate(entry, update)
                    ? CacheUpdateResult.Updated
                    : CacheUpdateResult.Rejected;
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        /// <inheritdoc />
        public async Task<bool> UpdateRewardQuestionsLockedAsync(
            int playerId,
            string loadSessionId,
            Func<CachedPlayer, CachedQuestion, uint?> update,
            CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(
                playerId,
                loadSessionId,
                ct);
            if (entry is null) return false;

            await entry.Lock.WaitAsync(ct);
            try
            {
                return ApplyQuestionUpdate(entry, update);
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        private static bool ApplyQuestionUpdate(
            CacheEntry entry,
            Func<CachedPlayer, CachedQuestion, uint?> update)
        {
            var dirtyMask = update(entry.Player, entry.CachedQ);
            if (dirtyMask is null)
                return false;

            entry.CachedQ.DirtyMask |= dirtyMask.Value;
            entry.LastAccessUtc = DateTime.UtcNow;
            return true;
        }

        /// <inheritdoc />
        public async Task<CacheUpdateResult> LogoutLockedRequestAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default)
        {
            if (!_entries.TryGetValue(playerId, out var entry))
                return CacheUpdateResult.NotFound;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return CacheUpdateResult.SessionMismatch;

                if (entry.HasAnyDirty == false)
                {
                    _entries.TryRemove(playerId, out _);
                }
                else
                {
                    entry.Dirty |= DirtyFlags.Logout;
                    entry.LastAccessUtc = DateTime.UtcNow;
                }

                return CacheUpdateResult.Updated;
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        /// <inheritdoc />
        public async Task DiscardAsync(
            int playerId,
            CancellationToken ct = default)
        {
            if (!_entries.TryGetValue(playerId, out var entry))
                return;

            await entry.Lock.WaitAsync(ct);
            try
            {
                ((ICollection<KeyValuePair<int, CacheEntry>>)_entries)
                    .Remove(new KeyValuePair<int, CacheEntry>(playerId, entry));
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        /// <inheritdoc />
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
                               | DirtyFlags.OrientStats
                               | DirtyFlags.TeamStats);

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

        /// <inheritdoc />
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
