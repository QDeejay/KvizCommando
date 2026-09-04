using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Enums;
using KvizCommando.Shared.Models.Rules;
using System.Text.Json;

namespace KvizCommando.Server.Services.DtoMapping
{
    partial class QuestionService
    {
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
                        maxUserSlot == 0
                            ? 0
                            : Math.Max(1, maxUserSlot >> 1),
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
    }
}
