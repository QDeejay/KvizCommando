using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Models;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.VsGame;
using KvizCommando.Server.Services.VsGame.Matchmaking;
using KvizCommando.Server.Utilities;
using KvizCommando.Server.Utilities.Recruit;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums;
using KvizCommando.Shared.Models.Rules;
using KvizCommando.Shared.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace KvizCommando.Server.Services.DtoMapping
{
    partial class ScreenService
    {
        /// <inheritdoc />
        public async Task<VsGameDtos?> GetVsGameScreenAsync(int playerId, string sessionId, CancellationToken ct = default)
        {
            var cacheResult = await _cache.GetOrLoadLockedAsync(
                playerId,
                sessionId,
                ct);

            if (cacheResult.Status == CacheReadStatus.SessionMismatch)
                return new VsGameDtos { AccessDenied = true };

            var player = cacheResult.Player;

            if (player is null)
            {
                _logger.LogWarning(
                    "Player not found in cache. userId={UserId}",
                    playerId);
                return null;
            }

            var rankedPlayerCounts =
                _rankedQueue.GetConnectedPlayerCounts();

            var teamMembers = player.Characters
                .Select((member, index) => new
                {
                    Member = member,
                    SlotNumber = index + 1
                })
                .Where(item => item.Member is not null)
                .Select(item => new VsBattleMemberDto
                {
                    SlotNumber = item.SlotNumber,
                    Name = item.Member!.Name,
                    PictureCode = item.Member.PictureCode,
                    Rank = item.Member.Rank,
                    RankClass =
                        VsBattleClassificationRules.ResolveRankClass(
                            item.Member.Rank),
                    OrientationId =
                        item.Member.Attitude.Main.CatNo[0] > 8
                            ? item.Member.Attitude.Main.CatNo[0] - 8
                            : item.Member.Attitude.Main.CatNo[0],
                    EnergyPoints = item.Member.EnergyPoints,
                    IsSelectable =
                        VsBattleClassificationRules.CanSelectMember(
                            player.Core.RankEnum,
                            item.Member.EnergyPoints,
                            item.Member.Rank,
                            item.Member.XP)
                })
                .ToArray();

            var battleReadyMemberCount =
                teamMembers.Count(member => member.IsSelectable);

            var savedSlots = player.BattleTeamSlots is null
                ? []
                : player.BattleTeamSlots.ToArray();

            var savedRanks = savedSlots.Length > 0 &&
                             savedSlots.All(slot => slot is >= 1 and <= 8)
                ? savedSlots
                    .Select(slot => player.Characters[slot - 1])
                    .Where(member =>
                        member is not null &&
                        VsBattleClassificationRules.CanSelectMember(
                            player.Core.RankEnum,
                            member.EnergyPoints,
                            member.Rank,
                            member.XP))
                    .Select(member => member!.Rank)
                    .ToArray()
                : [];

            var eligibleIds = savedRanks.Length == savedSlots.Length
                ? VsBattleClassificationRules
                    .GetEligibleClassificationIds(
                        player.Core.RankEnum,
                        savedRanks)
                : [];

            return new VsGameDtos
            {
                RootBoxInfo = new VsRootBoxInfo
                {
                    IsCreateBattlefieldEnabled = false,
                    IsJoinBattlefieldEnabled = false,
                    IsRankedBattlefieldsEnabled =
                        battleReadyMemberCount >= VsBattleClassificationRules.REQUIRED_BATTLE_READY_CHARACTERS &&
                        player.Core.Credit >= VsBattleClassificationRules.REQUIRED_CREDIT_BALANCE,
                    BattleReadyCharacterCount = battleReadyMemberCount,
                    RequiredBattleReadyCharacterCount = VsBattleClassificationRules.REQUIRED_BATTLE_READY_CHARACTERS,
                    CreditBalance = player.Core.Credit,
                    RequiredCreditBalance = VsBattleClassificationRules.REQUIRED_CREDIT_BALANCE,
                    TeamRank = player.Core.RankEnum,
                    PrivatePlayerCount = 0,
                    RankedPlayerCount =
                        rankedPlayerCounts.Values.Sum(),
                    RankedHighScore =
                        player.TeamStats.RankedHighScore
                },
                RankedBattlefields =
                    new VsRankedBattlefieldsDto
                    {
                        TeamMembers = teamMembers,
                        SavedSelection = new VsRankedSelectionDto
                        {
                            SelectedSlotNumbers = savedSlots,
                            EligibleClassificationIds = eligibleIds
                        },
                        Classifications =
                        [
                            .. VsBattleClassificationRules.List.Select(
                                rule => new VsBattleClassificationDto
                                {
                                    ClassificationId =
                                        rule.ClassificationId,
                                    Stake = rule.Stake,
                                    MinimumTeamRank =
                                        rule.MinimumTeamRank,
                                    RequiredPartySize =
                                        rule.RequiredPartySize,
                                    MemberMinimumRankClass =
                                        rule.MemberMinimumRankClass,
                                    MemberMaximumRankClass =
                                        rule.MemberMaximumRankClass,
                                    RequiredMembersInRankClassRange =
                                        rule.RequiredMembersInRankClassRange,
                                    PlayerCount =
                                        rankedPlayerCounts[
                                            rule.ClassificationId]
                                })
                        ]
                    }
            };
        }
    }
}
