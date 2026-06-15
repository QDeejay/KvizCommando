using KvizCommando.Client.Data;
using KvizCommando.Server.Data.StaticData;
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.Players;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.User;
using System.Text.Json;
using KvizCommando.Shared.Models;

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

        public async Task<bool> SaveFactorySlotsAsync(int playerId, SaveFactoryRequest dto, CancellationToken ct)
        {
            var sessionId = "Teszt";
            var loadout = new PlayerLoadout
            {
                UserSlotsJson = null, // nem módosítjuk
                PendingSlotsJson = null, // nem módosítjuk
                FactorySlotsJson = JsonSerializer.Serialize(dto.CategorySlots)
            };
            
            var success = await _cache.UpdatePartialLoadoutLockedAsync(
                playerId,
                sessionId,
                loadout,
                ct);
            return success;
        }
        public async Task<bool> ManageSlotsAsync(int playerId, ManageSlotRequest dto, CancellationToken ct)
        {
            var sessionId = "Teszt";

            
            ///
            /// Data validácio a cacheben
            /// 
            var (player, question) = await _cache.GetOrLoadLockedAsync(playerId, sessionId, ct);
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
                    if (dto.SlotNo > maxPendingSlot || freeUserSlot==0)
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
                sessionId,
                dto,
                ct
                );

            return succes;
        }
        public async Task<bool> SendNewQuestionAsync(int playerId, NewQuestionRequest dto, CancellationToken ct)
        {
            var sessionId = "Teszt";

            var (player, question) = await _cache.GetOrLoadLockedAsync(playerId, sessionId, ct);
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
                _logger.LogWarning($"DeletePending: Invalid user slot number. userId={playerId}, SlotNo={dto.SlotNo}",dto.SlotNo);
                return false;
            }
            var succes = await _cache.UpdatePartialNewQuestionLockedAsync(
                playerId,
                sessionId,
                dto,
                ct
                );


        return true;
        }
    }
}


 
