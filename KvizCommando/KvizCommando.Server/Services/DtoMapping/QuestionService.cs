using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Utilities;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using System.Text.Json;

namespace KvizCommando.Server.Services.DtoMapping
{
    public sealed class QuestionService : IQuestionService
    {
        private readonly IPlayerCacheService _cache;


        private readonly ILogger<QuestionService> _logger;

        public QuestionService(
            IPlayerCacheService cache,
            ILogger<QuestionService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<bool?> SaveFactorySlotsAsync(int playerId, SaveFactoryRequest dto, CancellationToken ct)
        {
            //var sessionId = "Teszt";
            //var sessionId = sessionid;
            var loadout = new PlayerLoadout
            {
                UserSlotsJson = null, // nem módosítjuk
                PendingSlotsJson = null, // nem módosítjuk
                FactorySlotsJson = JsonSerializer.Serialize(dto.CategorySlots)
            };

            var success = await _cache.UpdatePartialLoadoutLockedAsync(
                playerId,
                dto.SessionId,
                loadout,
                ct);
            return success;
        }
        public async Task<bool?> ManageSlotsAsync(int playerId, ManageSlotRequest dto, CancellationToken ct)
        {
            //var sessionId = "Teszt";
            //var sessionId = sessionid;
            //
            /// Data validácio a cacheben
            /// 
            var (player, question) = await _cache.GetOrLoadLockedAsync(playerId, dto.SessionId, ct);
            if (player == null)
                return false;
            if (player?.SessionId == "denied")
                return null;
            var lvl = player?.Core.RankEnum ?? 0;
            var maxUsrSlot = RankRewards.List[lvl].OwnQuestSlot;
            var maxPendingSlot = maxUsrSlot >> 1;
            var freeUserSlot = question.uSlots.Take(10).Count(x => x.CategoryNo == 0);
            var reqType = dto.ReqType;
            switch (reqType)
            {
                case SlotManageType.DeleteUsr:
                    if (dto.SlotNo > maxUsrSlot)
                    {
                        _logger.LogWarning($"DeleteUsr: Invalid user slot number. userId={playerId}, SlotNo={dto.SlotNo}", playerId, dto.SlotNo);
                        return false;
                    }

                    break;
                case SlotManageType.DeletePending:
                    if (dto.SlotNo > maxPendingSlot)
                    {
                        _logger.LogWarning($"DeletePending: Invalid user slot number. userId={playerId}, SlotNo={dto.SlotNo}", playerId, dto.SlotNo);
                        return false;
                    }

                    break;
                case SlotManageType.MovePending:
                    if (dto.SlotNo > maxPendingSlot || freeUserSlot == 0)
                    {
                        _logger.LogWarning($"MovePending: Invalid user slot number. userId={playerId}, SlotNo={dto.SlotNo}", playerId, dto.SlotNo);
                        return false;
                    }
                    break;
                default:
                    _logger.LogWarning("ManageSlots: Invalid request type. userId={playerId}, ReqType={dto.SlotNo}", playerId, dto.ReqType);
                    return false;
            }
            var succes = await _cache.UpdatePartialQuestionsLockedAsync(
                playerId,
                dto.SessionId,
                dto,
                ct
                );

            return succes;
        }
        public async Task<bool?> SendNewQuestionAsync(int playerId, NewQuestionRequest dto, CancellationToken ct)
        {
            //var sessionId = "Teszt";
            //var sessionId = sessionid;

            var (player, question) = await _cache.GetOrLoadLockedAsync(playerId, dto.SessionId, ct);
            if (player == null)
                return false;
            if (player.SessionId == "denied")
                return null;
            var freependingslot = question.pSlots.Take(5).Count(x => x.CategoryNo == 0);
            if (freependingslot == 0)
            {
                _logger.LogWarning($"SendNewQuestion: No free pending slot. userId={playerId}");
                return false;
            }
            var lvl = player?.Core.RankEnum ?? 0;
            var maxPendingSlot = RankRewards.List[lvl].OwnQuestSlot >> 1;
            if (dto.SlotNo > maxPendingSlot)
            {
                _logger.LogWarning($"SendNewQuestion: Invalid user slot number. userId={playerId}, SlotNo={dto.SlotNo}", dto.SlotNo);
                return false;
            }
            var succes = await _cache.UpdatePartialNewQuestionLockedAsync(
                playerId,
                dto.SessionId,
                dto,
                ct
                );


            return succes;
        }

        public async Task<QuestionDtos?> GetQuestionScreenAsync(int playerId, string sessionId, CancellationToken ct = default)
        {
            var (player, slot) = await _cache.GetOrLoadLockedAsync(playerId, sessionId, ct);

            if (player is null)
            {
                _logger.LogWarning("Player not found in cache. userId={UserId}", playerId);
                return null;
            }

            if (slot is null)
            {
                _logger.LogWarning("Question data is not founded. userId={UserId}", playerId);
                return null;
            }

            if (player.SessionId == "denied")
                return new QuestionDtos { AccessDenied = true };

            var context = BuildContext(player, slot);

            await CorrectFactorySlotsIfNeededAsync(
                    playerId,
                    sessionId,
                    context,
                    ct);


            var extendedInfo = new QuestionExtendedInfo
            {
                AvailablePendingSlot = context.AvailablePendingSlot,
                AvailableUserSlot = context.AvailableUserSlot,
                FreeUserSlot = context.FreeUserSlot,
                FreePendingSlot = context.FreePendingSlot,
                OccupiedUserSlot = context.OccupiedUserSlot,
                OccupiedPendingSlot = context.OccupiedPendingSlot,
                HandlePendingSlot = context.MovablePendingCount + context.RejectedPendingCount,
                UserSlotEnable = context.UserSlotEnable,
                NoFownQuestion = context.OwnQuestionCount,
                CharCatMask = player.CharCatMask
            };

            return new QuestionDtos
            {
                FactorySlots = context.FactorySlots,
                Userlots = context.UserSlots,
                PendingSlots = context.PendingSlots,
                ExtendedInfo = extendedInfo
            };
        }
        private static QuestionContext BuildContext(CachedPlayer player, CachedQuestion slot)
        {
            var level = player.Core.RankEnum;
            var rewards = RankRewards.List[level];

            var context = new QuestionContext
            {
                AvailableUserSlot = rewards.OwnQuestSlot,
                UserSlotEnable = level > 0
            };

            context.AvailablePendingSlot = context.AvailableUserSlot >> 1;

            context.FactorySlots = player.Loadout?.FactorySlotsJson.ConvertToArray<int>() ?? [];
            context.OwnQuestionCount = context.FactorySlots.Count(c => c == 17);

            context.UserSlots = BuildUserSlots(slot);
            context.PendingSlots = BuildPendingSlots(slot);

            context.CategoryMask =
                [.. player.CharCatMask, .. player.CharCatMask];

            context.FreeUserSlot =
                context.UserSlots.Take(10).Count(v => v.Category == 0);

            context.FreePendingSlot =
                context.PendingSlots.Take(5).Count(v => v.Category == 0);

            context.OccupiedUserSlot =
                context.AvailableUserSlot - context.FreeUserSlot;

            context.OccupiedPendingSlot =
                context.AvailablePendingSlot - context.FreePendingSlot;

            context.MovablePendingCount =
                context.PendingSlots.Take(5).Count(v => v is { Status: "Approved" });

            context.RejectedPendingCount =
                context.PendingSlots.Take(5).Count(v => v is { Status: "Rejected" });

            return context;
        }
        private static UserSlot[] BuildUserSlots(CachedQuestion slot)
        {
            var userSlots = new List<UserSlot>();

            foreach (var uq in slot.uSlots)
            {
                var answers = uq.AnswersJson.ConvertToArray<string>();

                userSlots.Add(new UserSlot
                {
                    Question = uq?.Question ?? string.Empty,
                    Answer = answers,
                    Category = uq?.CategoryNo ?? 0,
                    NoOfUse = uq.Ask > 0 ? uq.Ask.ToString() : "N/A",
                    NofOfCorrect = uq.Ask > 0 ? uq.OkAnswer.ToString() : "N/A",
                    Ratio = uq.Ask > 40
                        ? $"{(Math.Truncate(uq.Ratio * 1000) / 10):0.0}%"
                        : "N/A"
                });
            }

            return userSlots.ToArray();
        }
        private static PendingSlot[] BuildPendingSlots(CachedQuestion slot)
        {
            var pendingSlots = new List<PendingSlot>();

            foreach (var pq in slot.pSlots)
            {
                var answers = pq.AnswersJson.ConvertToArray<string>();

                pendingSlots.Add(new PendingSlot
                {
                    Question = pq?.Question ?? string.Empty,
                    Answer = answers,
                    Category = pq?.CategoryNo ?? 0,
                    Status = pq?.Status.ToString() ?? "None",
                    Remark = pq?.Remark,
                    SubmittedAt = pq?.SubmittedAt ?? DateTime.UtcNow
                });
            }

            return pendingSlots.ToArray();
        }

        private async Task CorrectFactorySlotsIfNeededAsync(
            int playerId,
            string sessionId,
            QuestionContext context,
            CancellationToken ct)
        {
            bool[] maskCurrent = new bool[16];
            bool[] maskChecked = new bool[16];

            foreach (var n in context.FactorySlots)
            {
                if (n is > 0 and < 17)
                    maskCurrent[n - 1] = true;
            }

            for (var i = 0; i < 16; i++)
            {
                maskChecked[i] = maskCurrent[i] && context.CategoryMask[i];
            }

            if (context.OccupiedUserSlot >= context.OwnQuestionCount && maskCurrent.SequenceEqual(maskChecked))
                return;

            var ownQuestionCounter = 0;

            for (var i = 0; i < context.FactorySlots.Length; i++)
            {
                if (context.FactorySlots[i] == 17)
                    ownQuestionCounter++;

                Console.WriteLine("------------------------------------------------------");
                Console.WriteLine($"Slot:{i} Category:{context.FactorySlots[i]} a j értéke:{ownQuestionCounter}");

                context.FactorySlots[i] =
                    context.FactorySlots[i] == 17 && ownQuestionCounter > context.OccupiedUserSlot
                        ? 0
                        : context.FactorySlots[i];
                if (context.FactorySlots[i] > 0 && context.FactorySlots[i] < 17)
                    context.FactorySlots[i] = !context.CategoryMask[context.FactorySlots[i]]
                                                   ? 0
                                                   : context.FactorySlots[i];

                Console.WriteLine($"Slot:{i} Category:{context.FactorySlots[i]} a j értéke:{ownQuestionCounter}");
                Console.WriteLine("------------------------------------------------------");
            }

            await _cache.UpdatePartialLoadoutLockedAsync(
                playerId,
                sessionId,
                new PlayerLoadout
                {
                    FactorySlotsJson = JsonSerializer.Serialize(context.FactorySlots)
                },
                ct);
        }

        private sealed class QuestionContext
        {
            internal int[] FactorySlots { get; set; } = [];
            internal bool[] CategoryMask { get; set; } = [];
            internal UserSlot[] UserSlots { get; set; } = [];
            internal PendingSlot[] PendingSlots { get; set; } = [];
            internal int AvailableUserSlot { get; set; }
            internal int AvailablePendingSlot { get; set; }
            internal bool UserSlotEnable { get; set; }
            internal int OwnQuestionCount { get; set; }
            internal int FreeUserSlot { get; set; }
            internal int FreePendingSlot { get; set; }
            internal int OccupiedUserSlot { get; set; }
            internal int OccupiedPendingSlot { get; set; }
            internal int MovablePendingCount { get; set; }
            internal int RejectedPendingCount { get; set; }
        }

    }
}



