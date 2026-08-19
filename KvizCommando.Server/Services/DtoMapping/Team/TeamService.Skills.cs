using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Utilities;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Rules;
using System.Text.Json;

namespace KvizCommando.Server.Services.DtoMapping
{
    partial class TeamService
    {
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
    }
}
