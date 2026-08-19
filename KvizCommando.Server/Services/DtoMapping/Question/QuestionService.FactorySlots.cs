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
    }
}
