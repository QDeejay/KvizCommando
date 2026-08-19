using KvizCommando.Server.Models;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Utilities;
using KvizCommando.Server.Utilities.Recruit;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Rules;
using System.Text.Json;

namespace KvizCommando.Server.Services.DtoMapping
{
    public class TeamService : ITeamService

    {
        private readonly IPlayerCacheService _cache;

        private readonly ILogger<TeamService> _logger;
        public TeamService(
            IPlayerCacheService cache,
            ILogger<TeamService> logger)
        {
            _cache = cache;
            _logger = logger;
        }


        /// <inheritdoc />
        public async Task<CacheUpdateResult> SaveModifiedSkillAsync(int playerid, ModifySkillRequest dto, CancellationToken ct = default)
        {
            return await _cache.UpdatePlayerLockedAsync(
                playerid,
                dto.SessionId,
                player =>
                {
                    if (dto.MemberId > 0 && player.CharCatMask[dto.MemberId - 1] == false)
                        return null;

                    var member = dto.MemberId > 0
                        ? player.Characters[dto.MemberId - 1]
                        : null;

                    if (dto.MemberId > 0 && member is null)
                        return null;

                    if (member is not null &&
                        !TeamRules.HasVitality(member.EnergyPoints))
                    {
                        return null;
                    }

                    int availableDevPoints = dto.MemberId == 0
                        ? player.Core.DevPoint
                        : member!.DevPoints;

                    var requestedLevels = dto.SkillChanges.Sum();
                    var totalUsedPoints = dto.MemberId == 0
                        ? requestedLevels * TeamRules.HELP_LEVEL_TEAM_DEV_POINT_COST
                        : requestedLevels;

                    if (availableDevPoints < totalUsedPoints)
                        return null;

                    int skillType = dto.MemberId == 0
                        ? 12
                        : dto.SkillType == 1 ? 4 : 8;

                    int maxLevel = dto.MemberId == 0
                        ? player.Core.RankEnum
                        : member!.Rank;

                    var helpDatas = string.IsNullOrEmpty(player.Loadout.HelpLevelsJson)
                        ? []
                        : JsonSerializer.Deserialize<int[]>(player.Loadout.HelpLevelsJson) ?? [];

                    for (int i = 0; i < 4; i++)
                    {
                        if (dto.SkillChanges[i] > 0 &&
                            maxLevel < RankConstants.startLevels[i + skillType])
                            return null;

                        int levelLimit = Math.Min(
                            RankConstants.maxLevels[i + skillType],
                            RankConstants.maxLevels[i + skillType] - 21 + maxLevel);
                        levelLimit = Math.Max(0, levelLimit);

                        if (dto.MemberId == 0)
                        {
                            if (dto.SkillChanges[i] > 0 &&
                                dto.SkillChanges[i] + helpDatas[i] > levelLimit)
                                return null;

                            helpDatas[i] += dto.SkillChanges[i];
                        }
                        else if (dto.SkillChanges[i] > 0 &&
                                 dto.SkillType == 0 &&
                                 dto.SkillChanges[i] + member!.Attitude.Secondary.Level[i] > levelLimit)
                        {
                            return null;
                        }
                        else if (dto.SkillChanges[i] > 0 &&
                                 dto.SkillType == 4 &&
                                 dto.SkillChanges[i] + member!.Attitude.Gender.Level[i] > levelLimit)
                        {
                            return null;
                        }
                    }

                    if (dto.MemberId > 0)
                    {
                        member!.DevPoints -= totalUsedPoints;

                        if (dto.SkillType == 1)
                            member.Attitude.Secondary.Level =
                                member.Attitude.Secondary.Level.AddTo(dto.SkillChanges);

                        if (dto.SkillType == 2)
                            member.Attitude.Gender.Level =
                                member.Attitude.Gender.Level.AddTo(dto.SkillChanges);

                        return DirtyFlags.Characters;
                    }

                    player.Core.DevPoint -= totalUsedPoints;
                    player.Loadout.HelpLevelsJson = JsonSerializer.Serialize(helpDatas);

                    return DirtyFlags.Core | DirtyFlags.Loadout;
                },
                ct);
        }

        /// <inheritdoc />
        public async Task<CacheUpdateResult> ManageTeamAsync(int playerid, ManageTeamRequest dto, CancellationToken ct = default)
        {
            return await _cache.UpdatePlayerLockedAsync(
                playerid,
                dto.SessionId,
                player => ManageTeam(player, dto),
                ct);
        }

        private static DirtyFlags? ManageTeam(
            CachedPlayer player,
            ManageTeamRequest dto)
        {
            var memberIndex = dto.MemberNo - 1;
            var member = player.Characters[memberIndex];
            var candidate = player.CandidateCharacters[memberIndex];

            if ((int)dto.ReqType > 0 && member is null)
                return null;

            if ((int)dto.ReqType <= 0 && candidate is null)
                return null;

            if (dto.ReqType is ManageType.Promote or ManageType.Retire &&
                !TeamRules.HasVitality(member!.EnergyPoints))
            {
                return null;
            }

            var dirty = DirtyFlags.Characters;

            switch (dto.ReqType)
            {
                case ManageType.Hire:
                    if (!CanHire(player, dto.MemberNo, candidate!))
                        return null;

                    member = Hire(dto, candidate!);
                    candidate = null;
                    break;

                case ManageType.Promote:
                    if (!CanPromote(player, member!))
                        return null;

                    Promote(player, member!);
                    dirty |= DirtyFlags.Core;
                    break;

                case ManageType.Retire:
                    if (!CanRetire(player, member!))
                        return null;

                    candidate = Retire(player, member!);
                    member = null;
                    dirty |= DirtyFlags.Core;
                    break;

                case ManageType.Fire:
                    if (TeamRules.HasVitality(member!.EnergyPoints))
                        return null;

                    member = null;
                    candidate = CreateFireReplacement();
                    break;

                case ManageType.Heal:
                    if (!CanHeal(member!))
                        return null;

                    Heal(member!);
                    break;

                default:
                    return null;
            }

            player.Characters[memberIndex] = member;
            player.CharCatMask[memberIndex] = member is not null;
            player.CandidateCharacters[memberIndex] = candidate;

            return dirty;
        }

        private static bool CanHire(
            CachedPlayer player,
            int memberNo,
            RecruitSlot candidate) =>
            candidate.Names is not null &&
            candidate.PictureCodes is not null &&
            !player.CharCatMask[memberNo - 1];

        private static bool CanPromote(
            CachedPlayer player,
            CharachterSlot member)
        {
            var rankClassChanged =
                TeamRules.IsRankClassChangingPromotion(member.Rank);

            return member.Rank < 21 &&
                   member.Rank < player.Core.RankEnum &&
                   (rankClassChanged ||
                    player.Core.DevPoint >=
                    TeamRules.PROMOTION_TEAM_DEV_POINT_COST) &&
                   member.XP >= RankRewards.List[member.Rank].NextLevelMember;
        }

        private static bool CanRetire(
            CachedPlayer player,
            CharachterSlot member) =>
            member.Rank == TeamRules.LAST_MEMBER_LEVEL &&
            player.Core.RankEnum > TeamRules.LAST_MEMBER_LEVEL &&
            member.XP >= RankRewards.List[21].NextLevelMember;

        private static bool CanHeal(CharachterSlot member) =>
            !TeamRules.HasVitality(member.EnergyPoints) &&
            member.DevPoints >= TeamRules.HEAL_CHARACTER_DEV_POINT_COST;

        private static CharachterSlot Hire(
            ManageTeamRequest dto,
            RecruitSlot candidate)
        {
            var recruit = RecruitService.RecruitResolver(
                dto.MemberNo,
                dto.CandidateId);
            var levels = new[] { 0, 0, 0, 0 };

            return new CharachterSlot
            {
                Name = candidate.Names![dto.CandidateId - 1],
                PictureCode = candidate.PictureCodes![dto.CandidateId - 1],
                XP = 0,
                Rank = TeamRules.HIRED_CHAR_STARTLEVEL,
                DevPoints = 0,
                EnergyPoints = TeamRules.GetMemberMaxVitality(0),
                Attitude = new Attitude
                {
                    Main = new AttitudeBranch
                    {
                        CatNo = recruit.Item1,
                        Level = levels
                    },
                    Secondary = new AttitudeBranch
                    {
                        CatNo = recruit.Item2,
                        Level = levels
                    },
                    Gender = new AttitudeBranch
                    {
                        CatNo = recruit.Item3,
                        Level = levels
                    }
                }
            };
        }

        private static void Promote(
            CachedPlayer player,
            CharachterSlot member)
        {
            var rankClassChanged =
                TeamRules.IsRankClassChangingPromotion(member.Rank);

            member.Rank = Math.Min(member.Rank + 1, 21);
            player.Core.DevPoint -= rankClassChanged
                ? 0
                : TeamRules.PROMOTION_TEAM_DEV_POINT_COST;
            member.DevPoints +=
                RankRewards.List[member.Rank].DevPointRevard;
            member.EnergyPoints =
                TeamRules.GetMemberMaxVitality(member.Rank);
            player.Core.DevPoint +=
                RankRewards.List[member.Rank].DevPointToStore;
        }

        private static RecruitSlot Retire(
            CachedPlayer player,
            CharachterSlot member)
        {
            member.Rank = Math.Min(member.Rank + 1, 21);
            var candidate = RecruitService.Generate(8, 7);
            candidate.ExpirationTime = DateTime.UtcNow.AddDays(7);
            player.Core.DevPoint += RankRewards.List[
                TeamRules.RETIRE_REWARD_RANK].DevPointToStore;
            player.Core.Credit += member.Pension;

            return candidate;
        }

        private static RecruitSlot CreateFireReplacement() => new()
        {
            Names = null,
            PictureCodes = null,
            ExpirationTime = DateTime.UtcNow.AddDays(
                TeamRules.FIRE_RECRUIT_DELAY_DAYS)
        };

        private static void Heal(CharachterSlot member)
        {
            member.EnergyPoints =
                TeamRules.GetMemberMaxVitality(member.Rank);
            member.DevPoints -= TeamRules.HEAL_CHARACTER_DEV_POINT_COST;
        }






    }
}
