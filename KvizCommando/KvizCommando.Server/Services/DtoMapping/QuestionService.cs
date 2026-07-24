using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Utilities;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums;
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
            return await _cache.UpdatePlayerLockedAsync(
                playerId,
                dto.SessionId,
                player =>
                {
                    player.Loadout.FactorySlotsJson =
                        JsonSerializer.Serialize(dto.CategorySlots);

                    return DirtyFlags.Loadout;
                },
                ct);
        }

        public async Task<bool?> ManageSlotsAsync(int playerId, ManageSlotRequest dto, CancellationToken ct)
        {
            return await _cache.UpdateQuestionsLockedAsync(
                playerId,
                dto.SessionId,
                (player, question) =>
                {
                    var level = player.Core.RankEnum;

                    var maxUserSlot = Math.Min(
                        RankRewards.List[level].OwnQuestSlot,
                        question.uSlots.Length);

                    var maxPendingSlot = Math.Min(
                        maxUserSlot >> 1,
                        question.pSlots.Length);

                    switch (dto.ReqType)
                    {
                        case SlotManageType.DeleteUsr:
                            if (dto.SlotNo < 0 || dto.SlotNo >= maxUserSlot)
                            {
                                _logger.LogWarning(
                                    "DeleteUsr: Invalid user slot number. userId={PlayerId}, SlotNo={SlotNo}",
                                    playerId,
                                    dto.SlotNo);
                                return null;
                            }

                            var userId = question.uSlots[dto.SlotNo].Id;
                            question.fSlots.Add(new FactoryQuestion
                            {
                                Id = 0,
                                Question = question.uSlots[dto.SlotNo].Question,
                                AnswersJson = question.uSlots[dto.SlotNo].AnswersJson,
                                CategoryNo = question.uSlots[dto.SlotNo].CategoryNo
                            });
                            question.uSlots[dto.SlotNo] = new UserQuestion
                            {
                                Id = userId,
                                PlayerId = playerId
                            };
                            return 1u << dto.SlotNo;

                        case SlotManageType.DeletePending:
                            if (dto.SlotNo < 0 || dto.SlotNo >= maxPendingSlot)
                            {
                                _logger.LogWarning(
                                    "DeletePending: Invalid pending slot number. userId={PlayerId}, SlotNo={SlotNo}",
                                    playerId,
                                    dto.SlotNo);
                                return null;
                            }

                            var pendingId = question.pSlots[dto.SlotNo].Id;
                            question.pSlots[dto.SlotNo] = new PendingQuestion
                            {
                                Id = pendingId,
                                PlayerId = playerId
                            };
                            return 1u << (dto.SlotNo + 16);

                        case SlotManageType.MovePending:
                            {
                                if (dto.SlotNo < 0 || dto.SlotNo >= maxPendingSlot)
                                {
                                    _logger.LogWarning(
                                        "MovePending: Invalid pending slot number. userId={PlayerId}, SlotNo={SlotNo}",
                                        playerId,
                                        dto.SlotNo);

                                    return null;
                                }

                                var firstEmptySlot = Array.FindIndex(
                                    question.uSlots,
                                    0,
                                    maxUserSlot,
                                    slot => slot.CategoryNo == 0);

                                if (firstEmptySlot < 0)
                                    return null;

                                var firstEmptyId = question.uSlots[firstEmptySlot].Id;
                                var movedPendingId = question.pSlots[dto.SlotNo].Id;

                                question.uSlots[firstEmptySlot] = new UserQuestion
                                {
                                    Id = firstEmptyId,
                                    PlayerId = playerId,
                                    Question = question.pSlots[dto.SlotNo].Question,
                                    AnswersJson = question.pSlots[dto.SlotNo].AnswersJson,
                                    CategoryNo = question.pSlots[dto.SlotNo].CategoryNo
                                };

                                question.pSlots[dto.SlotNo] = new PendingQuestion
                                {
                                    Id = movedPendingId,
                                    PlayerId = playerId
                                };

                                return (1u << firstEmptySlot) |
                                       (1u << (dto.SlotNo + 16));
                            }

                        default:
                            _logger.LogWarning(
                                "ManageSlots: Invalid request type. userId={PlayerId}, ReqType={ReqType}",
                                playerId,
                                dto.ReqType);
                            return null;
                    }
                },
                ct);
        }

        public async Task<bool?> SendNewQuestionAsync(int playerId, NewQuestionRequest dto, CancellationToken ct)
        {
            return await _cache.UpdateQuestionsLockedAsync(
                playerId,
                dto.SessionId,
                (player, question) =>
                {
                    var freePendingSlots = question.pSlots
                        .Take(5)
                        .Count(item => item.CategoryNo == 0);

                    if (freePendingSlots == 0)
                    {
                        _logger.LogWarning(
                            "SendNewQuestion: No free pending slot. userId={PlayerId}",
                            playerId);
                        return null;
                    }

                    var maxPendingSlot =
                        RankRewards.List[player.Core.RankEnum].OwnQuestSlot >> 1;

                    if (dto.SlotNo > maxPendingSlot)
                    {
                        _logger.LogWarning(
                            "SendNewQuestion: Invalid pending slot number. userId={PlayerId}, SlotNo={SlotNo}",
                            playerId,
                            dto.SlotNo);
                        return null;
                    }

                    var id = question.pSlots[dto.SlotNo].Id;
                    question.pSlots[dto.SlotNo] = new PendingQuestion
                    {
                        Id = id,
                        PlayerId = playerId,
                        Question = dto.Question,
                        AnswersJson = JsonSerializer.Serialize(dto.Answers),
                        CategoryNo = dto.Category,
                        Status = (QuestionStatus)1,
                        SubmittedAt = DateTime.UtcNow
                    };

                    return 1u << (dto.SlotNo + 16);
                },
                ct);
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

            context.UserSlots = BuildUserSlots(slot, context.AvailableUserSlot);

            context.PendingSlots = BuildPendingSlots(slot, context.AvailablePendingSlot);

            context.CategoryMask =
                [.. player.CharCatMask, .. player.CharCatMask];

            context.FreeUserSlot =
                context.UserSlots.Count(slot => slot.Category == 0);

            context.FreePendingSlot =
                context.PendingSlots.Count(slot => slot.Category == 0);

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
        private static UserSlot[] BuildUserSlots(CachedQuestion slot, int limit)
        {
            var userSlots = new List<UserSlot>();

            foreach (var uq in slot.uSlots.Take(limit))
            {
                var answers = uq.AnswersJson.ConvertToArray<string>();

                userSlots.Add(new UserSlot
                {
                    Question = uq.Question ?? string.Empty,
                    Answer = answers,
                    Category = uq.CategoryNo,
                    NoOfUse = uq.Ask > 0 ? uq.Ask.ToString() : "N/A",
                    NofOfCorrect = uq.Ask > 0 ? uq.OkAnswer.ToString() : "N/A",
                    Ratio = uq.Ask > 40
                        ? $"{(Math.Truncate(uq.Ratio * 1000) / 10):0.0}%"
                        : "N/A"
                });
            }

            return [.. userSlots];
        }
        private static PendingSlot[] BuildPendingSlots(CachedQuestion slot, int limit)
        {
            var pendingSlots = new List<PendingSlot>();

            foreach (var pq in slot.pSlots.Take(limit))
            {
                var answers = pq.AnswersJson.ConvertToArray<string>();

                pendingSlots.Add(new PendingSlot
                {
                    Question = pq.Question ?? string.Empty,
                    Answer = answers,
                    Category = pq.CategoryNo,
                    Status = pq.Status.ToString(),
                    Remark = pq.Remark,
                    SubmittedAt = pq.SubmittedAt
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
                    context.FactorySlots[i] = !context.CategoryMask[context.FactorySlots[i] - 1]
                                                   ? 0
                                                   : context.FactorySlots[i];

                Console.WriteLine($"Slot:{i} Category:{context.FactorySlots[i]} a j értéke:{ownQuestionCounter}");
                Console.WriteLine("------------------------------------------------------");
            }

            await _cache.UpdatePlayerLockedAsync(
                playerId,
                sessionId,
                player =>
                {
                    player.Loadout.FactorySlotsJson =
                        JsonSerializer.Serialize(context.FactorySlots);

                    return DirtyFlags.Loadout;
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

