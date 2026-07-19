using KvizCommando.Server.Models;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Utilities.Recruit;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums;
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


        public async Task<bool?> SaveModifiedSkillAsync(int playerid, ModifySkillRequest dto, CancellationToken ct = default)
        {
            //var sessionId = "Teszt";
            var (player, _) = await _cache.GetOrLoadLockedAsync(playerid, dto.SessionId, ct);
            if (player == null)
                return false;
            if (player.SessionId == "denied")
                return null;
            if (dto.MemberId > 0 && player.CharCatMask[dto.MemberId - 1] == false)
                return false;

            int avialableDevPoints = dto.MemberId == 0 ? player.Core.DevPoint : player.Characters[dto.MemberId - 1].DevPoints;

            if (avialableDevPoints < dto.SkillChanges.Sum())
                return false;

            int skillType = dto.MemberId == 0 ? 12 : dto.SkillType == 1 ? 4 : 8;
            int maxLevel = dto.MemberId == 0 ? player.Core.RankEnum : player.Characters[dto.MemberId - 1].Rank;
            int levelLimit;
            var helpDatas = string.IsNullOrEmpty(player.Loadout?.HelpLevelsJson)
                    ? []
                     : JsonSerializer.Deserialize<int[]>(player.Loadout.HelpLevelsJson) ?? [];
            string newHelpJson = player.Loadout.HelpLevelsJson;
            // validálás 
            for (int i = 0; i < 4; i++)
            {
                if (dto.SkillChanges[i] > 0 && maxLevel < RankConstants.startLevels[i + skillType]) return false;
                levelLimit = Math.Min(RankConstants.maxLevels[i + skillType], RankConstants.maxLevels[i + skillType] - 21 + maxLevel);
                levelLimit = Math.Max(0, levelLimit);
                if (dto.MemberId == 0)
                {
                    if (dto.SkillChanges[i] > 0 && dto.SkillChanges[i] + helpDatas[i] > levelLimit)
                        return false;
                    helpDatas[i] += dto.SkillChanges[i];

                }
                else if (dto.SkillChanges[i] > 0 && dto.SkillType == 0 && dto.SkillChanges[i] + player.Characters[dto.MemberId - 1].Attitude.Secondary.Level[i] > levelLimit)
                    return false;
                else if (dto.SkillChanges[i] > 0 && dto.SkillType == 4 && dto.SkillChanges[i] + player.Characters[dto.MemberId - 1].Attitude.Gender.Level[i] > levelLimit)
                    return false;
            }
            newHelpJson = JsonSerializer.Serialize(helpDatas);

            int totalUsedPoints = dto.SkillChanges.Sum();
            int modifiedMemberId = dto.MemberId; // 0=team, 1-8=characters

            Console.WriteLine($"Modifying skills for player {playerid}, member {modifiedMemberId}, total used points {totalUsedPoints}");

            return await _cache.UpdatePartialModifySillsLockedAsync(playerid, dto.SessionId, newHelpJson, dto, ct);
        }
        public async Task<bool?> ManageTeamAsync(int playerid, ManageTeamRequest dto, CancellationToken ct = default)
        {
            //ar sessionId = "Teszt";
            var (player, question) = await _cache.GetOrLoadLockedAsync(playerid, dto.SessionId, ct);
            if (player == null)
                return false;
            if (player.SessionId == "denied")
                return null;
            var member = player.Characters[dto.MemberNo - 1];
            var candidate = player.CandidateCharacters[dto.MemberNo - 1];

            var level = member != null ? member.Rank : 0;
            var teamLevel = player.Core.RankEnum;
            var devPoints = player.Core.DevPoint;
            var nextLevelXp = RankRewards.List[level].NextLevel;
            if ((int)dto.ReqType > 0)
            { if (member == null) return false; }
            else
            { if (candidate == null) return false; }

            switch (dto.ReqType)
            {
                case ManageType.Hire:
                    if (candidate.Names == null || candidate.PictureCodes == null)
                        return false;
                    if (player.CharCatMask[dto.MemberNo - 1])
                        return false;
                    break;
                case ManageType.Promote:
                    {
                        var rankClassChanged = ((level - 1) / 3 + 1) != ((level) / 3 + 1) || level == 0;
                        if (level >= 21 || level >= teamLevel)
                            return false;
                        if (devPoints == 0 && rankClassChanged != false)
                            return false;
                        if (member.XP < nextLevelXp)
                            return false;
                        break;
                    }
                case ManageType.Retire:
                    if (level != 21 || teamLevel < 22)
                        return false;
                    if (member.XP < RankRewards.List[21].NextLevel)
                        return false;
                    break;
                case ManageType.Fire:
                    if (member.EnergyPoints > 0)
                        return false;
                    break;
                case ManageType.Heal:
                    if (member.EnergyPoints > 0 || member.DevPoints <= 0)
                        return false;
                    break;
                default:
                    return false;
            }
            var succesPlayerUpdate = _cache.UpdatePartialCharachtersAsync(playerid, dto.SessionId, dto, null, ct);

            return await succesPlayerUpdate;
        }
        public async Task<TeamDtos?> GetTeamScreenDataAsync(int playerId, string sessionId, CancellationToken ct = default)
        {
            var (player, _) = await _cache.GetOrLoadLockedAsync(playerId, sessionId, ct);

            if (player is null)
            {
                _logger.LogWarning("Player not found in cache. userId={UserId}", playerId);
                return null;
            }

            if (player.SessionId == "denied")
                return new TeamDtos { AccessDenied = true };

            //player.CandidateCharacters = await CorrectCandidateSlotsAsync(playerId, sessionId, player, ct);

            var context = BuildContext(player);

            await CorrectCandidateSlotsAsync(playerId, sessionId, player, context, ct);

            var teamInfo = new TeamExtendedInfo
            {
                Name = player.Core.TeamName,
                Level = player.Core.RankEnum,
                Xp = player.Core.XP,
                NextXp = RankRewards.List[player.Core.RankEnum].NextLevel,
                DevPoints = player.Core.DevPoint,
                TotalMembers = context.NumberOfCharacters,
                MaxMembers = RankRewards.List[player.Core.RankEnum].MaxCharacters,
                AbleToHireMask = context.AbleToHireMask,
                Bonus = RankRewards.List[player.Core.RankEnum].WinBonus,
                Credits = player.Core.Credit,
                MembRemarks = context.MemberRemarks
            };

            var help = HelpResolver(
                player.Loadout.HelpLevelsJson,
                teamInfo.Level,
                teamInfo.DevPoints);

            var rootBoxInfo = TeamRootBoxInfoResolver(
                teamInfo,
                help.CanDev);

            return new TeamDtos
            {
                TeamInfo = teamInfo,
                TeamMembers = context.TeamMembers,
                Candidates = context.Candidates,
                CharCatMask = context.CharacterMask,
                Help = help,
                RootBoxInfo = rootBoxInfo
            };
        }

        private static TeamContext BuildContext(CachedPlayer player)
        {
            var context = new TeamContext();
            context.TeamMembers[0] = null;
            context.Candidates[0] = null;
            context.CharacterMask[0] = true;
            context.AbleToHireMask[0] = false;

            BuildMembers(player, context);
            BuildCandidates(player, context);

            return context;
        }
        private static HelpDto HelpResolver(string helpDatasJson, int rank, int teamDevPoints)
        {
            var helpDatas = string.IsNullOrEmpty(helpDatasJson)
               ? [0, 0, 0, 0, 0, 0, 0, 0]
               : JsonSerializer.Deserialize<int[]>(helpDatasJson) ?? [0, 0, 0, 0, 0, 0, 0, 0];

            var helpSkill = SkillResolver(helpDatas[0..4], rank, RankConstants.maxLevels[12..16], RankConstants.startLevels[12..16]);

            return new HelpDto
            {
                Skill = helpSkill,
                CanDev = helpSkill.Any(x => x.SkillCanDev) && teamDevPoints > 0,
                HelpVolumes = [.. helpDatas.Skip(4).Take(4)],
                Category = [101, 102, 103, 104]
            };
        }
        private static TeamRootBoxInfo TeamRootBoxInfoResolver(TeamExtendedInfo info, bool helpDev)
        {
            var retire = info.MembRemarks.Count(x => x == MembRemark.Retire);
            var fire = info.MembRemarks.Count(x => x == MembRemark.Fire);
            var promote = info.MembRemarks.Count(x => x == MembRemark.Promote);
            var heal = info.MembRemarks.Count(x => x == MembRemark.Heal);
            var help = helpDev ? 1 : 0;
            var freePositions = Math.Max(info.MaxMembers - info.TotalMembers, 0);
            var ableToHire = info.AbleToHireMask.Count(x => x);

            return new TeamRootBoxInfo
            {
                IsTeamEnable = true,
                IsRecruitEnable = freePositions > 0,
                IsMemberEnable = info.TotalMembers > 0,
                TeamOpRequired = retire + fire + promote + heal + help,
                MemberOpRequired = info.MembRemarks.Count(x => x == MembRemark.Develop),
                FreePositions = freePositions,
                AbleToHire = ableToHire
            };
        }

        private static void BuildMembers(CachedPlayer player, TeamContext context)
        {
            for (int i = 1; i < 9; i++)
            {
                var character = player.Characters[i - 1];

                if (character != null)
                {
                    int tempRank = character.Rank;
                    int nextRank = Math.Min(21, character.Rank + 1);

                    context.TeamMembers[i] = new TeamMemberDto
                    {
                        Name = character.Name,
                        Level = tempRank,
                        PictureCode = character.PictureCode,
                        Xp = character.XP,
                        Pension = character.Pension,
                        SkillPoints = character.DevPoints,
                        EnergyPoints = character.EnergyPoints,
                        NextXp = RankRewards.List[tempRank].NextLevel,
                        NextDevPoints = tempRank != nextRank ? RankRewards.List[nextRank].NextLevel : 0,
                        NextUnlock = tempRank != nextRank ? RankRewards.List[nextRank].NextLevel : null,
                        MaintAttitude = AttitudeResolver(
                            character.Attitude.Main,
                            tempRank,
                            RankConstants.maxLevels[0..4],
                            RankConstants.startLevels[0..4],
                            0),

                        SecondAttitude = AttitudeResolver(
                            character.Attitude.Secondary,
                            tempRank,
                            RankConstants.maxLevels[4..8],
                            RankConstants.startLevels[4..8],
                            character.DevPoints),

                        GenderAttitude = AttitudeResolver(
                            character.Attitude.Gender,
                            tempRank,
                            RankConstants.maxLevels[8..12],
                            RankConstants.startLevels[8..12],
                            character.DevPoints)
                    };

                    context.MemberRemarks[i] = RemarkResolver(
                        context.TeamMembers[i]!,
                        player.Core.DevPoint,
                        player.Core.RankEnum);

                    context.TeamMembers[i]!.Remark = context.MemberRemarks[i];

                    context.CharacterMask[i] = true;
                    context.NumberOfCharacters++;
                }
                else
                {
                    context.TeamMembers[i] = null;
                    context.CharacterMask[i] = false;
                }
            }
        }
        private static void BuildCandidates(CachedPlayer player, TeamContext context)
        {
            /*
             for (int i = 0; i < 8; i++)
            {
                if (cp.CharCatMask[i])
                    cp.CandidateCharacters[i] = null;
                else
                {
                    if (cp.CandidateCharacters[i] == null || DateTime.UtcNow > cp.CandidateCharacters[i].ExpirationTime)
                    {
                        cp.CandidateCharacters[i] = RecruitService.Generate(8) ?? new RecruitSlot();
                        cp.CandidateCharacters[i].ExpirationTime = DateTime.UtcNow.AddDays(7);
                        entry.Dirty = DirtyFlags.Characters;
                    }
                }
            }
             */
            // var updatedRecruitSlot = new List<RecruitSlot?>();

            for (int i = 1; i < 9; i++)
            {
                if (context.CharacterMask[i])
                {
                    //updatedRecruitSlot.Add(null);
                    context.Candidates[i] = new CandidateDto { CanBeHire = false, ExpirationTime = DateTime.UtcNow.AddMonths(1) };
                    context.UpdRecruitSlots[i - 1] = null;
                }
                else
                {
                    var candidate = player.CandidateCharacters[i - 1];

                    if (candidate == null || DateTime.UtcNow > candidate.ExpirationTime)
                        candidate = RecruitService.Generate(8, 7) ?? new RecruitSlot();

                    //updatedRecruitSlot.Add(candidate ?? new RecruitSlot());
                    context.Candidates[i] = new CandidateDto
                    {
                        Name = candidate.Names,
                        PictureCode = candidate.PictureCodes,
                        CanBeHire = candidate.Names != null &&
                                    candidate.PictureCodes != null,
                        ExpirationTime = candidate.ExpirationTime
                    };
                    context.UpdRecruitSlots[i - 1] = candidate;

                }
                context.AbleToHireMask[i] = context.Candidates[i]!.CanBeHire;

            }
        }


        private static AttidtudeDto AttitudeResolver(AttitudeBranch attitude, int rank, int[] maxLevels, int[] startLevels, int devPoints)
        {
            var skill = SkillResolver(attitude.Level, rank, maxLevels, startLevels, [0, 1, 0, 1]);
            return new AttidtudeDto
            {
                Category = [.. attitude.CatNo.Take(4).Select(x => (byte)x)],
                Skill = skill,
                CanDev = skill.Any(x => x.SkillCanDev) && devPoints > 0
            };
        }
        private static SkillPartial[] SkillResolver(int[] data, int mainActLevel, int[] constMaxLev, int[] constStartLev, int[]? correctors = null)
        {
            correctors ??= [0, 0, 0, 0];
            var sp = new SkillPartial[data.Length];

            for (int i = 0; i < data.Length; i++)
                sp[i] = SkillPartialResolver(data[i], mainActLevel, constMaxLev[i], constStartLev[i] - 1, correctors[i]);

            return sp;
        }
        private static SkillPartial SkillPartialResolver(int currentLevel, int actualRank, int maxLevel, int startmodifier, int corrector)
        {
            int maxmodify = Math.Max(0, actualRank - startmodifier);
            int maxlevel = Math.Min(maxLevel, maxmodify);
            maxlevel = Math.Max(0, maxlevel);
            return new SkillPartial
            {
                LvlCurrent = (byte)currentLevel,
                LvlCurMax = maxlevel > 0 ? (byte)(maxlevel + corrector) : (byte)maxlevel,
                LvlOvrMax = maxlevel > 0 ? (byte)(maxlevel + corrector) : (byte)maxlevel,
                SkillCanDev = (byte)currentLevel < (byte)maxlevel
            };
        }
        private static MembRemark RemarkResolver(TeamMemberDto mem, int teamPoints, int teamLevel)
        {
            if (mem.EnergyPoints <= 0)
                return mem.SkillPoints > 0 ? MembRemark.Heal : MembRemark.Fire;

            if (mem.NextXp <= mem.Xp && mem.Level < teamLevel)
                if (mem.Level == 21)
                    return MembRemark.Retire;
                else if (teamPoints > 0)
                    return MembRemark.Promote;

            if (mem.SkillPoints > 0 && (mem.SecondAttitude.CanDev || mem.GenderAttitude.CanDev))
                return MembRemark.Develop;

            return MembRemark.None;
        }

        private async Task CorrectCandidateSlotsAsync(
            int playerId,
            string sessionId,
            CachedPlayer player,
            TeamContext context,
            CancellationToken ct)
        {
            bool updateRequired = false;

            for (int i = 0; i < 8; i++)
            {
                updateRequired |= context.UpdRecruitSlots[i] != player.CandidateCharacters[i];
            }

            if (updateRequired)
                await _cache.UpdatePartialCharachtersAsync(playerId, sessionId, new ManageTeamRequest { }, context.UpdRecruitSlots, ct);

            return;
        }

        private sealed class TeamContext
        {
            internal TeamMemberDto?[] TeamMembers { get; } = new TeamMemberDto?[9];
            internal CandidateDto[] Candidates { get; } = new CandidateDto[9];
            internal RecruitSlot?[] UpdRecruitSlots { get; } = new RecruitSlot?[8];
            internal MembRemark[] MemberRemarks { get; } = new MembRemark[9];
            internal bool[] CharacterMask { get; } = new bool[9];
            internal bool[] AbleToHireMask { get; } = new bool[9];
            internal int NumberOfCharacters { get; set; }
        }
    }
}

