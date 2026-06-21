using KvizCommando.Client.Models.DataModels;
using KvizCommando.Server.Data.StaticData;
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Models;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.IdentityModel.Tokens.Experimental;
using System.Text.Json;
using KvizCommando.Shared.Models;

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
            var (player, question) = await _cache.GetOrLoadLockedAsync(playerid, dto.SessionId, ct);
            if (player == null)
                return false;
            if (player.SessionId == "denied")
                return null;
            if (dto.MemberId>0 && player.CharCatMask[dto.MemberId-1] == false)
                return false;
            
            int avialableDevPoints = dto.MemberId == 0 ? player.Core.DevPoint : player.Characters[dto.MemberId-1].DevPoints;
            if (avialableDevPoints < dto.SkillChanges.Sum())
                return false;

            int skillType = dto.MemberId == 0 ? 12 : dto.SkillType == 1 ? 4 : 8;
            int maxLevel = dto.MemberId == 0 ? player.Core.RankEnum : player.Characters[dto.MemberId - 1].Rank;
            int levelLimit;
            var helpDatas = string.IsNullOrEmpty(player.Loadout?.HelpLevelsJson)
                    ? Array.Empty<int>()
                     : JsonSerializer.Deserialize<int[]>(player.Loadout.HelpLevelsJson) ?? Array.Empty<int>();
            string newHelpJson = player.Loadout.HelpLevelsJson;
            // validálás 
            for (int i = 0; i < 4; i++)
            {
                if (dto.SkillChanges[i]>0 && maxLevel < RankConstants.startLevels[i+skillType]) return false;
                levelLimit = Math.Min(RankConstants.maxLevels[i + skillType], RankConstants.maxLevels[i + skillType]-21+maxLevel);
                levelLimit = Math.Max(0, levelLimit);
                if (dto.MemberId == 0)
                {
                    if (dto.SkillChanges[i]>0 && dto.SkillChanges[i] + helpDatas[i] > levelLimit)
                        return false;
                    helpDatas[i] += dto.SkillChanges[i];

                }
                else if (dto.SkillChanges[i] > 0 && dto.SkillType == 0 && dto.SkillChanges[i] + player.Characters[dto.MemberId].Attitude.Secondary.Level[i] > levelLimit)
                    return false;
                else if (dto.SkillChanges[i] > 0 && dto.SkillType == 4 && dto.SkillChanges[i] + player.Characters[dto.MemberId].Attitude.Gender.Level[i] > levelLimit)
                    return false;
            }
            newHelpJson = JsonSerializer.Serialize(helpDatas);
            int totalUsedPoints = dto.SkillChanges.Sum();
            int modifiedMemberId = dto.MemberId; // 0=team, 1-8=characters
            Console.WriteLine($"Modifying skills for player {playerid}, member {modifiedMemberId}, total used points {totalUsedPoints}");
            return await _cache.UpdatePartialModifySillsLockedAsync(playerid, dto.SessionId, newHelpJson, dto, ct );
        }
        public async Task<bool?> ManageTeamAsync(int playerid, ManageTeamRequest dto, CancellationToken ct = default)
        {
            //ar sessionId = "Teszt";
            var (player, question) = await _cache.GetOrLoadLockedAsync(playerid, dto.SessionId, ct);
            if ( player==null)
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
                        var rankClassChanged = ((level - 1) / 3 + 1) != ((level) / 3 + 1) || level==0;
                        if (level >= 21 || level>=teamLevel)
                            return false;
                        if (devPoints == 0 && rankClassChanged!=false)
                            return false;
                        if (member.XP < nextLevelXp)
                            return false;
                        break;
                    }
                case ManageType.Retire:
                    if (level != 21 || teamLevel<22)
                        return false;
                    if (member.XP < RankRewards.List[21].NextLevel)
                        return false;
                    break;
                case ManageType.Fire:
                    if (member.EnergyPoints>0)
                        return false;
                    break;
                case ManageType.Heal:
                    if (member.EnergyPoints > 0 || member.DevPoints<=0)
                        return false;
                    break;
                default:
                    return false;
            }

            return await _cache.UpdatePartialCharachters(playerid, dto.SessionId, dto, ct);
        }
    }
}

