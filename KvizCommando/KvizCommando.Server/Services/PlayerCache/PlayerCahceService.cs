using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Models;
using KvizCommando.Server.Services.Db;
using KvizCommando.Server.Utilities;
using KvizCommando.Server.Utilities.Recruit;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Enums;
using System.Collections.Concurrent;
using System.Text.Json;


namespace KvizCommando.Server.Services.PlayerCache
{
    public sealed class PlayerCacheService : IPlayerCacheService
    {
        //private readonly ApplicationDbContext _db;
        private readonly IPlayerDbService _playerDb;
        private readonly IQuestionDbService _questionDb;
        private readonly IRecruitService _recruit;
        private static readonly ConcurrentDictionary<int, CacheEntry> _entries = new();


        public PlayerCacheService(//ApplicationDbContext db,
            IPlayerDbService playerdb,
            IQuestionDbService questiondb,
            IRecruitService recruit
            )
        {
            //_db = db;
            _playerDb = playerdb;
            _questionDb = questiondb;
            _recruit = recruit;
        }

        /// <summary>
        /// Az összes aktív user azonosítója, akik jelenleg bent vannak a cache-ben.
        /// </summary>
        public IReadOnlyCollection<int> GetActivePlayerIds()
            => _entries.Keys.ToList();

        // --------------------------------------------------------
        // PRIVÁT: DB-ből töltés (mindig csak CachedPlayer)
        // --------------------------------------------------------
        /*
        private async Task<CachedPlayer?> LoadFromDbAsync(
            int playerId,
            string sessionId,
            CancellationToken ct)
        {
            var player = await _db.Set<Player>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PlayerId == playerId, ct);

            if (player is null)
                return null;

            var loadout = await _db.Set<PlayerLoadout>()
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.PlayerId == playerId, ct);

            var characters = await _db.Set<PlayerCharacter>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.PlayerId == playerId, ct);
          

            var askStats = await _db.Set<PlayerAskStats>()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.PlayerId == playerId, ct);

            var categoryStats = await _db.Set<PlayerCategoryStat>()
                .AsNoTracking()
                .Where(cs => cs.PlayerId == playerId)
                .ToListAsync(ct);
            CharachterSlot?[] tempChars = characters is null
                        ? new CharachterSlot?[8]
                         : System.Text.Json.JsonSerializer
                        .Deserialize<CharachterSlot?[]>(characters.CharactersJson)!;
            RecruitSlot?[] tempCandidates = characters is null
                        ? new RecruitSlot?[8]
                         : System.Text.Json.JsonSerializer
                        .Deserialize<RecruitSlot?[]>(characters.CandidatesJson)!;

            bool[] tempCharMask = tempChars.Select(x => x != null).ToArray();
            bool dirtyFlagCandidates = false;
            for (int i = 0; i < 8; i++)
            {
                if (tempCharMask[i])
                    tempCandidates[i] = null;
                else
                {
                    if (tempCandidates[i] == null || DateTime.UtcNow > tempCandidates[i].ExpirationTime)
                    {
                        tempCandidates[i] = _recruit.Generate(8);
                        tempCandidates[i].ExpirationTime = DateTime.UtcNow.AddDays(7);
                        dirtyFlagCandidates = true;
                    }
                }
            }

            return new CachedPlayer
            {
                Core = player,
                Loadout = loadout ?? new PlayerLoadout
                {
                    PlayerId = playerId,
                    FactorySlotsJson = "[]",
                    UserSlotsJson = "[]",
                    PendingSlotsJson = "[]",
                    UpdatedUtc = DateTime.UtcNow
                },
                Characters = tempChars,
                CandidateCharacters = tempCandidates,
                CandidateChanged = dirtyFlagCandidates,
                CharCatMask = tempCharMask,
                AskStats = askStats ?? new PlayerAskStats
                {
                    PlayerId = playerId,
                    TotalQuestionsAsked = 0,
                    TotalAskPointsEarned = 0
                },
                CategoryStats = categoryStats,
                SessionId = sessionId
            };

        }
        */
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
            for (int i = 0; i < 8; i++)
            {
                if (cp.CharCatMask[i])
                    cp.CandidateCharacters[i] = null;
                else
                {
                    if (cp.CandidateCharacters[i] == null || DateTime.UtcNow > cp.CandidateCharacters[i].ExpirationTime)
                    {
                        cp.CandidateCharacters[i] = _recruit.Generate(8) ?? new RecruitSlot();
                        cp.CandidateCharacters[i].ExpirationTime = DateTime.UtcNow.AddDays(7);
                        entry.Dirty = DirtyFlags.Characters;
                    }
                }
            }

            //if (cp.CandidateChanged)
            //  entry.Dirty = DirtyFlags.Characters;
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

        public async Task<bool?> UpdatePartialCharachters(
            int playerId,
            string sessionId,
            ManageTeamRequest teamReq,
            CancellationToken ct = default
            )
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return false;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return null;
                var member = entry.Player.Characters[teamReq.MemberNo - 1] ?? new CharachterSlot();
                var candidate = entry.Player.CandidateCharacters[teamReq.MemberNo - 1] ?? new RecruitSlot();

                switch (teamReq.ReqType)
                {
                    case ManageType.Hire:
                        {
                            var rl = _recruit.RecruitResolver(teamReq.MemberNo, teamReq.CandidateId);
                            var sl = new int[4] { 0, 0, 0, 0 };
                            member = new CharachterSlot()
                            {
                                Name = candidate!.Names![teamReq.CandidateId - 1],
                                PictureCode = candidate.PictureCodes![teamReq.CandidateId - 1],
                                XP = 0,
                                Rank = 0,
                                DevPoints = 0,
                                EnergyPoints = 36,
                                Attitude = new Attitude()
                                {
                                    Main = new AttitudeBranch()
                                    {
                                        CatNo = rl.Item1,
                                        Level = sl
                                    },
                                    Secondary = new AttitudeBranch()
                                    {
                                        CatNo = rl.Item2,
                                        Level = sl
                                    },
                                    Gender = new AttitudeBranch()
                                    {
                                        CatNo = rl.Item3,
                                        Level = sl
                                    }
                                }
                            };
                            candidate = null;
                            break;
                        }
                    case ManageType.Promote:
                        {
                            var rankClassChanged = ((member.Rank - 1) / 3 + 1) != ((member.Rank) / 3 + 1) || member.Rank == 0;
                            member.Rank += 1;
                            member.Rank = Math.Min(member.Rank, 21);
                            entry.Player.Core.DevPoint -= rankClassChanged ? 0 : 1;
                            member.DevPoints += RankRewards.List[member.Rank].DevPointRevard;
                            member.EnergyPoints = 36 + member.Rank * 3;
                            entry.Player.Core.DevPoint += RankRewards.List[member.Rank].DevPointToStore;

                            break;
                        }
                    case ManageType.Retire:
                        {
                            member.Rank += 1;
                            member.Rank = Math.Min(member.Rank, 21);
                            candidate = _recruit.Generate(8);
                            candidate.ExpirationTime = DateTime.UtcNow.AddDays(7);
                            entry.Player.Core.DevPoint += RankRewards.List[member.Rank].DevPointToStore;
                            entry.Player.Core.Credit += member.Pension;
                            member = null;
                            break;
                        }
                    case ManageType.Fire:
                        {
                            member = null;
                            candidate = new RecruitSlot()
                            {
                                Names = null,
                                PictureCodes = null,
                                ExpirationTime = DateTime.UtcNow.AddDays(1)
                            };
                            break;
                        }
                    case ManageType.Heal:
                        member.EnergyPoints = 33 + member.Rank * 3;
                        member.DevPoints -= 1;
                        break;
                    default:
                        return false;
                }
                entry.Player.Characters[teamReq.MemberNo - 1] = member;
                entry.Player.CharCatMask[teamReq.MemberNo - 1] = member != null;
                entry.Player.CandidateCharacters[teamReq.MemberNo - 1] = candidate;
                entry.Dirty |= DirtyFlags.Characters;
                entry.LastAccessUtc = DateTime.UtcNow;
                return true;
            }

            finally
            {
                entry.Lock.Release();
            }

        }

        public async Task<bool?> UpdatePartialModifySillsLockedAsync(
           int playerId,
           string sessionId,
           string newhelpdata,
           ModifySkillRequest newskilldata,
           CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return false;
            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return null;
                if (newskilldata.MemberId > 0)
                {
                    entry.Player.Characters[newskilldata.MemberId - 1].DevPoints -= newskilldata.SkillChanges.Sum();
                    if (newskilldata.SkillType == 1)
                        entry.Player.Characters[newskilldata.MemberId - 1].Attitude.Secondary.Level = entry.Player.Characters[newskilldata.MemberId - 1].Attitude.Secondary.Level.AddTo(newskilldata.SkillChanges);
                    if (newskilldata.SkillType == 2)
                        entry.Player.Characters[newskilldata.MemberId - 1].Attitude.Gender.Level = entry.Player.Characters[newskilldata.MemberId - 1].Attitude.Gender.Level.AddTo(newskilldata.SkillChanges);
                    entry.Dirty |= DirtyFlags.Characters;
                }
                else
                {
                    entry.Player.Core.DevPoint -= newskilldata.SkillChanges.Sum();
                    entry.Player.Loadout.HelpLevelsJson = newhelpdata;
                    entry.Dirty |= DirtyFlags.Core;
                    entry.Dirty |= DirtyFlags.Loadout;
                }
                entry.LastAccessUtc = DateTime.UtcNow;
                return true;
            }
            finally
            {
                entry.Lock.Release();
            }
        }
        public async Task<bool?> UpdatePartialQuestionsLockedAsync(
           int playerId,
           string sessionId,
           ManageSlotRequest slotReq,
           CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return false;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return null;

                switch (slotReq.ReqType)
                {
                    case SlotManageType.DeleteUsr:
                        {
                            var tempId = entry.CachedQ.uSlots[slotReq.SlotNo].Id;
                            entry.CachedQ.fSlots.Add(new FactoryQuestion()
                            {
                                Id = 0,
                                Question = entry.CachedQ.uSlots[slotReq.SlotNo].Question,
                                AnswersJson = entry.CachedQ.uSlots[slotReq.SlotNo].AnswersJson,
                                CategoryNo = entry.CachedQ.uSlots[slotReq.SlotNo].CategoryNo,
                            });
                            entry.CachedQ.uSlots[slotReq.SlotNo] = new UserQuestion { Id = tempId, PlayerId = playerId };
                            entry.CachedQ.DirtyMask |= (1u << slotReq.SlotNo);
                            break;
                        }
                    case SlotManageType.DeletePending:
                        {
                            var tempId = entry.CachedQ.pSlots[slotReq.SlotNo].Id;
                            entry.CachedQ.pSlots[slotReq.SlotNo] = new PendingQuestion { Id = tempId, PlayerId = playerId };
                            entry.CachedQ.DirtyMask |= (1u << (slotReq.SlotNo + 16));
                            break;
                        }
                    case SlotManageType.MovePending:
                        {
                            var firstEmptySlot = Array.FindIndex(entry.CachedQ.uSlots, q => q.CategoryNo == 0);
                            if (firstEmptySlot > 9) return false;
                            var firstEmptyId = entry.CachedQ.uSlots[firstEmptySlot].Id;
                            var tempId = entry.CachedQ.pSlots[slotReq.SlotNo].Id;
                            entry.CachedQ.uSlots[firstEmptySlot] = new UserQuestion
                            {
                                Id = firstEmptyId,
                                PlayerId = playerId,
                                Question = entry.CachedQ.pSlots[slotReq.SlotNo].Question,
                                AnswersJson = entry.CachedQ.pSlots[slotReq.SlotNo].AnswersJson,
                                CategoryNo = entry.CachedQ.pSlots[slotReq.SlotNo].CategoryNo,
                            };
                            entry.CachedQ.pSlots[slotReq.SlotNo] = new PendingQuestion { Id = tempId, PlayerId = playerId };
                            entry.CachedQ.DirtyMask |= (1u << firstEmptySlot);
                            entry.CachedQ.DirtyMask |= (1u << (slotReq.SlotNo + 16));
                            break;
                        }
                    default:
                        return false;
                }

                entry.LastAccessUtc = DateTime.UtcNow;
                return true;
            }
            finally
            {
                entry.Lock.Release();
            }
        }
        public async Task<bool?> UpdatePartialNewQuestionLockedAsync(
           int playerId,
           string sessionId,
           NewQuestionRequest slotReq,
           CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return false;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return null;
                //var firstEmpty = Array.FindIndex(entry.Player.PendingQuestions, q => q.CategoryNo == 0);
                if (slotReq.SlotNo > 4) return false;
                var tempId = entry.CachedQ.pSlots[slotReq.SlotNo].Id;
                var tempjson = JsonSerializer.Serialize(slotReq.Answers);
                entry.CachedQ.pSlots[slotReq.SlotNo] = new PendingQuestion
                {
                    Id = tempId,
                    PlayerId = playerId,
                    Question = slotReq.Question,
                    AnswersJson = tempjson,
                    CategoryNo = slotReq.Category,
                    Status = (QuestionStatus)1,
                    SubmittedAt = DateTime.UtcNow
                };
                entry.CachedQ.DirtyMask |= (1u << slotReq.SlotNo + 16);
                entry.LastAccessUtc = DateTime.UtcNow;

                return true;
            }
            finally
            {
                entry.Lock.Release();
            }
        }

        // --------------------------------------------------------
        // Részleges frissítések
        // --------------------------------------------------------
        public async Task<bool?> UpdatePartialLoadoutLockedAsync(
            int playerId,
            string sessionId,
            PlayerLoadout newLoadout,
            CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return false;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return null;

                if (newLoadout.FactorySlotsJson != null)
                    entry.Player.Loadout.FactorySlotsJson = newLoadout.FactorySlotsJson;

                if (newLoadout.UserSlotsJson != null)
                    entry.Player.Loadout.UserSlotsJson = newLoadout.UserSlotsJson;

                if (newLoadout.PendingSlotsJson != null)
                    entry.Player.Loadout.PendingSlotsJson = newLoadout.PendingSlotsJson;

                entry.Dirty |= DirtyFlags.Loadout;
                entry.LastAccessUtc = DateTime.UtcNow;
                return true;
            }
            finally
            {
                entry.Lock.Release();
            }
        }


        public async Task<bool?> UpdatePartialAskStatsLockedAsync(
            int playerId,
            string sessionId,
            PlayerAskStats newStats,
            CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return false;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return null;

                entry.Player.AskStats = newStats;
                entry.Dirty |= DirtyFlags.AskStats;
                entry.LastAccessUtc = DateTime.UtcNow;
                return true;
            }
            finally
            {
                entry.Lock.Release();
            }
        }
        // kategoria statisztika frissités
        public async Task<bool?> UpdatePartialCategoryStatsLockedAsync(
            int playerId,
            string sessionId,
            List<PlayerCategoryStat> newStats,
            CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return false;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return null;

                entry.Player.CategoryStats = newStats;
                entry.Dirty |= DirtyFlags.CategoryStats;
                entry.LastAccessUtc = DateTime.UtcNow;
                return true;
            }
            finally
            {
                entry.Lock.Release();
            }
        }
        // Orientáció statisztika frissités
        public async Task<bool?> UpdatePartialOrientStatsLockedAsync(
            int playerId,
            string sessionId,
            List<PlayerOrientStat> newStats,
            CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return false;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return null;

                entry.Player.OrientStats = newStats;
                entry.Dirty |= DirtyFlags.OrientStats;
                entry.LastAccessUtc = DateTime.UtcNow;
                return true;
            }
            finally
            {
                entry.Lock.Release();
            }
        }




        // --------------------------------------------------------
        // Teljes frissítés
        // --------------------------------------------------------
        public async Task<bool?> UpdateAllLockedAsync(
            int playerId,
            string sessionId,
            CachedPlayer updated,
            CachedQuestion updatedQ,
            CancellationToken ct = default)
        {
            var entry = await GetOrCreateEntryAsync(playerId, sessionId, ct);
            if (entry is null) return false;

            await entry.Lock.WaitAsync(ct);
            try
            {
                if (entry.Player.SessionId != sessionId)
                    return null;

                entry.Player.Core = updated.Core;
                entry.Player.Loadout = updated.Loadout;
                entry.Player.Characters = updated.Characters;
                entry.Player.AskStats = updated.AskStats;
                entry.Player.CategoryStats = updated.CategoryStats;
                Array.Copy(updatedQ.uSlots, entry.CachedQ.uSlots, updatedQ.uSlots.Length);
                Array.Copy(updatedQ.pSlots, entry.CachedQ.pSlots, updatedQ.pSlots.Length);


                entry.Dirty = DirtyFlags.Core
                            | DirtyFlags.Loadout
                            | DirtyFlags.Characters
                            | DirtyFlags.AskStats
                            | DirtyFlags.CategoryStats
                            | DirtyFlags.OrientStats;
                uint mask1 = (1u << 10) - 1;              // 0–9. bit
                uint mask2 = ((1u << 5) - 1) << 16;       // 16–20. bit
                entry.CachedQ.DirtyMask = mask1 | mask2;
                entry.LastAccessUtc = DateTime.UtcNow;
                entry.CachedQ.DirtyMask = 0;
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

                await _playerDb.SavePlayerToDbAsync(entry.Player, entry.Dirty, playerId, ct);

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

