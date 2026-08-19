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
