using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Enums;
using KvizCommando.Shared.Models.Rules;
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

        /// <inheritdoc />
        public async Task<CacheUpdateResult> SaveFactorySlotsAsync(int playerId, SaveFactoryRequest dto, CancellationToken ct)
        {
            return await _cache.UpdatePlayerAndQuestionsLockedAsync(
                playerId,
                dto.SessionId,
                (player, question) =>
                {
                    var level = player.Core.RankEnum;
                    if (level <= 0)
                        return null;

                    var loadoutSize =
                        QuestionLoadoutRules.GetLoadoutSize(level);

                    if (dto.CategorySlots.Length < loadoutSize)
                        return null;

                    var categorySlots = dto.CategorySlots
                        .Take(loadoutSize)
                        .ToArray();

                    if (categorySlots.Any(category =>
                            category is < 0 or >
                                QuestionLoadoutRules.OWN_QUESTION_CATEGORY))
                    {
                        return null;
                    }

                    var maxUserSlot = Math.Min(
                        RankRewards.List[level].OwnQuestSlot,
                        question.uSlots.Length);

                    var occupiedUserSlots = question.uSlots
                        .Take(maxUserSlot)
                        .Count(slot => slot.CategoryNo > 0);

                    var ownQuestionLimit =
                        QuestionLoadoutRules.GetOwnQuestionLimit(
                            loadoutSize,
                            occupiedUserSlots);

                    if (categorySlots.Count(category =>
                            category ==
                                QuestionLoadoutRules.OWN_QUESTION_CATEGORY) >
                        ownQuestionLimit)
                    {
                        return null;
                    }

                    if (categorySlots
                        .Skip(loadoutSize / 2)
                        .Any(category =>
                            category ==
                                QuestionLoadoutRules.OWN_QUESTION_CATEGORY))
                    {
                        return null;
                    }

                    player.Loadout.FactorySlotsJson =
                        JsonSerializer.Serialize(categorySlots);

                    return DirtyFlags.Loadout;
                },
                ct);
        }

        /// <inheritdoc />
        public async Task<CacheUpdateResult> ManageSlotsAsync(int playerId, ManageSlotRequest dto, CancellationToken ct)
        {
            return await _cache.UpdateQuestionsLockedAsync(
                playerId,
                dto.SessionId,
                (player, question) =>
                {
                    var level = player.Core.RankEnum;

                    if (level <= 0)
                        return null;

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

        /// <inheritdoc />
        public async Task<CacheUpdateResult> SendNewQuestionAsync(int playerId, NewQuestionRequest dto, CancellationToken ct)
        {
            return await _cache.UpdateQuestionsLockedAsync(
                playerId,
                dto.SessionId,
                (player, question) =>
                {
                    if (player.Core.RankEnum <= 0)
                        return null;

                    var categoryMaskIndex = (dto.Category - 1) % 8;
                    if (dto.Category is < 1 or > 16 ||
                        categoryMaskIndex >= player.CharCatMask.Length ||
                        !player.CharCatMask[categoryMaskIndex])
                    {
                        _logger.LogWarning(
                            "SendNewQuestion: Category is not available. userId={PlayerId}, Category={Category}",
                            playerId,
                            dto.Category);
                        return null;
                    }

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

                    var maxPendingSlot = Math.Min(
                        RankRewards.List[player.Core.RankEnum].OwnQuestSlot >> 1,
                        question.pSlots.Length);

                    if (dto.SlotNo < 0 ||
                        dto.SlotNo >= maxPendingSlot)
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



    }
}
