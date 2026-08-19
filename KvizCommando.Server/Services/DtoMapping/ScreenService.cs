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
    public sealed class ScreenService : IScreenService
    {
        private readonly IPlayerCacheService _cache;
        private readonly IVsRankedQueueService _rankedQueue;
        private readonly ILogger<ScreenService> _logger;
        private readonly IWebHostEnvironment _env;

        public ScreenService(
            IPlayerCacheService cache,
            IVsRankedQueueService rankedQueue,
            ILogger<ScreenService> logger,
            IWebHostEnvironment env)
        {
            _cache = cache;
            _rankedQueue = rankedQueue;
            _logger = logger;
            _env = env;
        }

        /// <inheritdoc />
        public async Task<HomeDTOs?> GetHomeScreenAsync(int playerId, string sessionId, CancellationToken ct = default)
        {

            var cacheResult = await _cache.GetOrLoadLockedAsync(
                playerId,
                sessionId,
                ct);

            if (cacheResult.Status == CacheReadStatus.SessionMismatch)
                return new HomeDTOs { AccessDenied = true };

            var player = cacheResult.Player;

            if (player == null)
            {
                return null;
            }
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            var url = Path.Combine(_env.WebRootPath, "BulletinBoard", culture, "bb.html");

            var dt = File.GetLastWriteTimeUtc(url);
            // A darabszámok ugyanabból a karakterpillanatképből készülnek.
            int characterCount = 0;
            int activeChars = 0;

            var charValues = player.Characters;
            if (charValues is not null)
            {
                foreach (var ch in charValues)
                {
                    if (ch is null) continue;

                    characterCount++;

                    if (ch.EnergyPoints > 0 &&
                        !VsBattleClassificationRules
                            .IsAwaitingRetirement(ch.Rank, ch.XP))
                    {
                        activeChars++;
                    }
                }
            }

            var nextLevelXp = RankRewards.List[player.Core.RankEnum].NextLevelTeam;

            var extendedInfo = new HomeExtendedInfo
            {
                LastInfo = dt,
                NextXp = nextLevelXp
            };

            var homeScreen = new HomeScreen
            {
                // A még nem implementált modulok gombjai tiltottak; a hozzájuk tartozó
                // számlálók jelenleg kizárólag a felület statikus helykitöltő adatai.
                NavBarEnable = true,
                Team = new ScreenButtonEntity
                {
                    Enable = true,
                    FooterData1 = activeChars,
                    FooterData2 = RankRewards.List[player.Core.RankEnum].MaxCharacters
                },
                Question = new ScreenButtonEntity
                {
                    Enable = player.Core.RankEnum > 0 ||
                        RankRewards.List[player.Core.RankEnum].OwnQuestSlot > 0,
                },
                SoloGame = new ScreenButtonEntity
                {
                    Enable = characterCount > 0
                },
                VsGame = new ScreenButtonEntity
                {
                    Enable = characterCount > 2,
                    FooterData1 = _cache.GetActivePlayerIds().Count
                },
                Shop = new ScreenButtonEntity
                {
                    Enable = false
                },
                Statistic = new ScreenButtonEntity
                {
                    Enable = false
                },
                Community = new ScreenButtonEntity
                {
                    Enable = false
                },
                Messages = new ScreenButtonEntity
                {
                    Enable = false,
                    FooterData1 = 5,
                    FooterData2 = 3
                },
                Ranking = new ScreenButtonEntity
                {
                    Enable = false,
                    FooterData1 = 9
                },
                Events = new ScreenButtonEntity
                {
                    Enable = false,
                    FooterData1 = 99,
                    FooterData2 = 8
                },
                InfoBoard = new ScreenButtonEntity
                {
                    Enable = true,
                }

            };

            var userMain = new UserMainData
            {
                PlayerId = playerId,
                UserName = player.Core.DisplayName,
                TeamName = player.Core.TeamName,
                RankEnum = player.Core.RankEnum,
                XP = player.Core.XP,
                Credit = player.Core.Credit,
                Voucher = player.Core.Voucher
            };

            return new HomeDTOs
            {
                UserMainData = userMain,
                ExtendedInfo = extendedInfo,
                HomeScreen = homeScreen
            };
        }

        /// <inheritdoc />
        public async Task<TeamDtos?> GetTeamScreenDataAsync(int playerId, string sessionId, CancellationToken ct = default)
        {
            var cacheResult = await _cache.GetOrLoadLockedAsync(
                playerId,
                sessionId,
                ct);

            if (cacheResult.Status == CacheReadStatus.SessionMismatch)
                return new TeamDtos { AccessDenied = true };

            var player = cacheResult.Player;

            if (player is null)
            {
                _logger.LogWarning("Player not found in cache. userId={UserId}", playerId);
                return null;
            }


            var context = BuildContext(player);

            await CorrectCandidateSlotsAsync(playerId, sessionId, context, ct);

            var teamInfo = new TeamExtendedInfo
            {
                Name = player.Core.TeamName,
                Level = player.Core.RankEnum,
                Xp = player.Core.XP,
                NextXp = RankRewards.List[player.Core.RankEnum].NextLevelTeam,
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

        /// <inheritdoc />
        public async Task<QuestionDtos?> GetQuestionScreenAsync(int playerId, string sessionId, CancellationToken ct = default)
        {
            var cacheResult = await _cache.GetOrLoadLockedAsync(
                playerId,
                sessionId,
                ct);

            if (cacheResult.Status == CacheReadStatus.SessionMismatch)
                return new QuestionDtos { AccessDenied = true };

            var player = cacheResult.Player;
            var slot = cacheResult.Questions;

            if (player is null)
            {
                _logger.LogWarning("Player not found in cache. userId={UserId}", playerId);
                return null;
            }

            if (slot is null)
            {
                _logger.LogWarning("Question data is not founded. userId={UserId}", playerId);
                return null;
            }

            if (player.Core.RankEnum <= 0 &&
                RankRewards.List[player.Core.RankEnum].OwnQuestSlot <= 0)
                return new QuestionDtos { AccessDenied = true };

            var context = BuildContext(player, slot);

            await CorrectFactorySlotsIfNeededAsync(
                    playerId,
                    sessionId,
                    context,
                    ct);


            var extendedInfo = new QuestionExtendedInfo
            {
                AvailablePendingSlot = context.AvailablePendingSlot,
                AvailableUserSlot = context.AvailableUserSlot,
                FreeUserSlot = context.FreeUserSlot,
                FreePendingSlot = context.FreePendingSlot,
                OccupiedUserSlot = context.OccupiedUserSlot,
                OccupiedPendingSlot = context.OccupiedPendingSlot,
                HandlePendingSlot = context.MovablePendingCount + context.RejectedPendingCount,
                UserSlotEnable = context.UserSlotEnable,
                NoFownQuestion = context.OwnQuestionCount,
                CharCatMask = player.CharCatMask
            };

            return new QuestionDtos
            {
                FactorySlots = context.FactorySlots,
                Userlots = context.UserSlots,
                PendingSlots = context.PendingSlots,
                ExtendedInfo = extendedInfo
            };
        }


        /// <inheritdoc />
        public async Task<SoloGameDtos?> GetSoloGameScreenAsync(int playerId, string sessionId, CancellationToken ct = default)
        {
            var cacheResult = await _cache.GetOrLoadLockedAsync(
                playerId,
                sessionId,
                ct);

            if (cacheResult.Status == CacheReadStatus.SessionMismatch)
                return new SoloGameDtos { AccessDenied = true };

            var player = cacheResult.Player;

            if (player is null)
            {
                _logger.LogWarning("Player not found in cache. userId={UserId}", playerId);
                return null;
            }
            var mask = player.CharCatMask;
            var orientationMask =
                new bool[player.Characters.Length];

            for (var index = 0;
                 index < player.Characters.Length;
                 index++)
            {
                var character = player.Characters[index];

                if (character is null)
                    continue;

                orientationMask[index] = true;
            }
            var results = new SoloResults
            {
                OrientResults = GetOriResultFromCache(player.OrientStats),
                CategoryResults = GetCatResultFromCache(player.CategoryStats)

            };
            var enables = new SoloEnables
            {
                EnaCampaign = false,
                EnaCategory = mask.AsSpan().Contains(true),
                EnaOrient = orientationMask.AsSpan().Contains(true),
                EnaOri = orientationMask,
                EnaCat = mask.Concat(mask).ToArray(),
            };


            return new SoloGameDtos
            {
                Mask = mask,
                Results = results,
                Enables = enables
            };

        }

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

        private static ResultDto[] GetCatResultFromCache(List<PlayerCategoryStat> data)
        {
            int ix;
            double tempTime;
            var result = new ResultDto[data.Count + 1];
            result[0] = new ResultDto { Points = 0, Time = 0.0 };
            foreach (var d in data)
            {
                ix = Math.Min((int)d.CategoryId, 16);
                tempTime = Math.Round(d.HighScoreTime, 1);
                result[ix] = new ResultDto
                {
                    Points = d.HighScore,
                    Time = tempTime,
                    TimeStr = tempTime > 99.9 ? "+99.9" : tempTime.ToString("0.0", CultureInfo.InvariantCulture)
                };
                result[0].Points += d.HighScore;
                result[0].Time += tempTime;
            }
            return result;
        }
        private static ResultDto[] GetOriResultFromCache(List<PlayerOrientStat> data)
        {
            int ix;
            double tempTime;
            var result = new ResultDto[data.Count + 1];
            result[0] = new ResultDto { Points = 0, Time = 0.0 };
            foreach (var d in data)
            {
                ix = Math.Min((int)d.OrientId, 8);
                tempTime = Math.Round(d.HighScoreTime, 1);
                result[ix] = new ResultDto
                {
                    Points = d.HighScore,
                    Time = Math.Round(d.HighScoreTime, 1),
                    TimeStr = tempTime > 99.9 ? "+99.9" : tempTime.ToString("0.0", CultureInfo.InvariantCulture)
                };
                result[0].Points += d.HighScore;
                result[0].Time += Math.Round(d.HighScoreTime, 1);
            }
            return result;
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
                CanDev = helpSkill.Any(x => x.SkillCanDev) &&
                         teamDevPoints >= TeamRules.HELP_LEVEL_TEAM_DEV_POINT_COST,
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

        private async Task CorrectCandidateSlotsAsync(
          int playerId,
          string sessionId,
          TeamContext context,
          CancellationToken ct)
        {
            await _cache.UpdatePlayerLockedAsync(
                playerId,
                sessionId,
                player =>
                {
                    bool updateRequired = false;

                    for (int i = 0; i < 8; i++)
                    {
                        updateRequired |=
                            context.UpdRecruitSlots[i] != player.CandidateCharacters[i];
                    }

                    if (updateRequired == false)
                        return DirtyFlags.None;

                    player.CandidateCharacters = context.UpdRecruitSlots;
                    return DirtyFlags.Characters;
                },
                ct);
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

        private static void BuildMembers(CachedPlayer player, TeamContext context)
        {
            for (int i = 1; i < 9; i++)
            {
                var character = player.Characters[i - 1];

                if (character != null)
                {
                    int tempRank = character.Rank;
                    int nextRank = Math.Min(21, character.Rank + 1);
                    bool hasVitality = TeamRules.HasVitality(
                        character.EnergyPoints);

                    context.TeamMembers[i] = new TeamMemberDto
                    {
                        Name = character.Name,
                        Level = tempRank,
                        PictureCode = character.PictureCode,
                        Xp = character.XP,
                        Pension = character.Pension,
                        SkillPoints = character.DevPoints,
                        EnergyPoints = character.EnergyPoints,
                        NextHealingGameUtc = character.NextHealingGameUtc,
                        SoloBestScore = character.CharStatistic.SoloBestScore,
                        NextXp = RankRewards.List[tempRank].NextLevelMember,
                        MaintAttitude = AttitudeResolver(
                            character.Attitude.Main,
                            tempRank,
                            RankConstants.maxLevels[0..4],
                            RankConstants.startLevels[0..4],
                            0,
                            false),

                        SecondAttitude = AttitudeResolver(
                            character.Attitude.Secondary,
                            tempRank,
                            RankConstants.maxLevels[4..8],
                            RankConstants.startLevels[4..8],
                            character.DevPoints,
                            hasVitality),

                        GenderAttitude = AttitudeResolver(
                            character.Attitude.Gender,
                            tempRank,
                            RankConstants.maxLevels[8..12],
                            RankConstants.startLevels[8..12],
                            character.DevPoints,
                            hasVitality)
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
            for (int i = 1; i < 9; i++)
            {
                if (context.CharacterMask[i])
                {
                    context.Candidates[i] = new CandidateDto { CanBeHire = false, ExpirationTime = DateTime.UtcNow.AddMonths(1) };
                    context.UpdRecruitSlots[i - 1] = null;
                }
                else
                {
                    var candidate = player.CandidateCharacters[i - 1];

                    if (candidate == null || DateTime.UtcNow > candidate.ExpirationTime)
                        candidate = RecruitService.Generate(8, 7) ?? new RecruitSlot();

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

        private static AttidtudeDto AttitudeResolver(
           AttitudeBranch attitude,
           int rank,
           int[] maxLevels,
           int[] startLevels,
           int devPoints,
           bool canDevelop)
        {
            var skill = SkillResolver(attitude.Level, rank, maxLevels, startLevels, [0, 1, 0, 1]);

            if (!canDevelop)
            {
                foreach (var item in skill)
                    item.SkillCanDev = false;
            }

            return new AttidtudeDto
            {
                Category = [.. attitude.CatNo.Take(4).Select(x => (byte)x)],
                Skill = skill,
                CanDev = canDevelop &&
                         skill.Any(x => x.SkillCanDev) &&
                         devPoints > 0
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
            if (!TeamRules.HasVitality(mem.EnergyPoints))
                return mem.SkillPoints >= TeamRules.HEAL_CHARACTER_DEV_POINT_COST
                    ? MembRemark.Heal
                    : MembRemark.Fire;

            if (mem.NextXp <= mem.Xp && mem.Level < teamLevel)
                if (mem.Level == 21)
                    return MembRemark.Retire;
                else if (TeamRules.IsRankClassChangingPromotion(mem.Level) ||
                         teamPoints >= TeamRules.PROMOTION_TEAM_DEV_POINT_COST)
                    return MembRemark.Promote;

            if (mem.SkillPoints > 0 && (mem.SecondAttitude.CanDev || mem.GenderAttitude.CanDev))
                return MembRemark.Develop;

            return MembRemark.None;
        }


        private static QuestionContext BuildContext(CachedPlayer player, CachedQuestion slot)
        {
            var level = player.Core.RankEnum;
            var rewards = RankRewards.List[level];
            var storedFactorySlots =
                player.Loadout?.FactorySlotsJson.ConvertToArray<int>() ?? [];
            var loadoutSize =
                QuestionLoadoutRules.GetLoadoutSize(level);

            var context = new QuestionContext
            {
                AvailableUserSlot = Math.Min(
                    rewards.OwnQuestSlot,
                    slot.uSlots.Length),
                UserSlotEnable = level > 0,
                StoredFactorySlots = storedFactorySlots,
                FactorySlots = new int[loadoutSize]
            };

            Array.Copy(
                storedFactorySlots,
                context.FactorySlots,
                Math.Min(storedFactorySlots.Length, loadoutSize));

            context.AvailablePendingSlot = Math.Min(
                context.AvailableUserSlot >> 1,
                slot.pSlots.Length);

            context.OwnQuestionCount = context.FactorySlots.Count(c =>
                c == QuestionLoadoutRules.OWN_QUESTION_CATEGORY);

            context.UserSlots = BuildUserSlots(slot, context.AvailableUserSlot);

            context.PendingSlots = BuildPendingSlots(slot, context.AvailablePendingSlot);

            context.CategoryMask =
                [.. player.CharCatMask, .. player.CharCatMask];

            context.FreeUserSlot =
                context.UserSlots.Count(slot => slot.Category == 0);

            context.FreePendingSlot =
                context.PendingSlots.Count(slot => slot.Category == 0);

            context.OccupiedUserSlot =
                context.AvailableUserSlot - context.FreeUserSlot;

            context.OccupiedPendingSlot =
                context.AvailablePendingSlot - context.FreePendingSlot;

            context.MovablePendingCount =
                context.PendingSlots.Take(5).Count(v => v is { Status: "Approved" });

            context.RejectedPendingCount =
                context.PendingSlots.Take(5).Count(v => v is { Status: "Rejected" });

            return context;
        }
        private static UserSlot[] BuildUserSlots(CachedQuestion slot, int limit)
        {
            var userSlots = new List<UserSlot>();

            foreach (var uq in slot.uSlots.Take(limit))
            {
                var answers = uq.AnswersJson.ConvertToArray<string>();

                userSlots.Add(new UserSlot
                {
                    Question = uq.Question ?? string.Empty,
                    Answer = answers,
                    Category = uq.CategoryNo,
                    NoOfUse = uq.Ask > 0 ? uq.Ask.ToString() : "N/A",
                    NofOfCorrect = uq.Ask > 0 ? uq.OkAnswer.ToString() : "N/A",
                    Ratio = uq.Ask >= 10
                        ? $"{(Math.Truncate(uq.Ratio * 1000) / 10):0.0}%"
                        : "N/A"
                });
            }

            return [.. userSlots];
        }
        private static PendingSlot[] BuildPendingSlots(CachedQuestion slot, int limit)
        {
            var pendingSlots = new List<PendingSlot>();

            foreach (var pq in slot.pSlots.Take(limit))
            {
                var answers = pq.AnswersJson.ConvertToArray<string>();

                pendingSlots.Add(new PendingSlot
                {
                    Question = pq.Question ?? string.Empty,
                    Answer = answers,
                    Category = pq.CategoryNo,
                    Status = pq.Status.ToString(),
                    Remark = pq.Remark,
                    SubmittedAt = pq.SubmittedAt
                });
            }

            return pendingSlots.ToArray();
        }
        private async Task CorrectFactorySlotsIfNeededAsync(
            int playerId,
            string sessionId,
            QuestionContext context,
            CancellationToken ct)
        {
            var ownQuestionLimit =
                QuestionLoadoutRules.GetOwnQuestionLimit(
                    context.FactorySlots.Length,
                    context.OccupiedUserSlot);
            var ownQuestionCounter = 0;
            var ownQuestionAreaLength =
                context.FactorySlots.Length / 2;

            for (var i = 0; i < context.FactorySlots.Length; i++)
            {
                var category = context.FactorySlots[i];

                if (category ==
                    QuestionLoadoutRules.OWN_QUESTION_CATEGORY)
                {
                    ownQuestionCounter++;

                    if (i >= ownQuestionAreaLength ||
                        ownQuestionCounter > ownQuestionLimit)
                    {
                        context.FactorySlots[i] = 0;
                    }

                    continue;
                }

                if (category is < 0 or >
                        QuestionLoadoutRules.OWN_QUESTION_CATEGORY ||
                    category is > 0 and <
                        QuestionLoadoutRules.OWN_QUESTION_CATEGORY &&
                    !context.CategoryMask[category - 1])
                {
                    context.FactorySlots[i] = 0;
                }
            }

            context.OwnQuestionCount = context.FactorySlots.Count(category =>
                category == QuestionLoadoutRules.OWN_QUESTION_CATEGORY);

            if (context.StoredFactorySlots.SequenceEqual(
                    context.FactorySlots))
            {
                return;
            }

            await _cache.UpdatePlayerLockedAsync(
                playerId,
                sessionId,
                player =>
                {
                    player.Loadout.FactorySlotsJson =
                        JsonSerializer.Serialize(context.FactorySlots);

                    return DirtyFlags.Loadout;
                },
                ct);
        }

        private sealed class QuestionContext
        {
            internal int[] StoredFactorySlots { get; set; } = [];
            internal int[] FactorySlots { get; set; } = [];
            internal bool[] CategoryMask { get; set; } = [];
            internal UserSlot[] UserSlots { get; set; } = [];
            internal PendingSlot[] PendingSlots { get; set; } = [];
            internal int AvailableUserSlot { get; set; }
            internal int AvailablePendingSlot { get; set; }
            internal bool UserSlotEnable { get; set; }
            internal int OwnQuestionCount { get; set; }
            internal int FreeUserSlot { get; set; }
            internal int FreePendingSlot { get; set; }
            internal int OccupiedUserSlot { get; set; }
            internal int OccupiedPendingSlot { get; set; }
            internal int MovablePendingCount { get; set; }
            internal int RejectedPendingCount { get; set; }
        }

    }

}
