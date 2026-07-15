using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Models;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums;
using KvizCommando.Shared.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace KvizCommando.Server.Services.DtoMapping
{
    public sealed class ScreenService : IScreenService
    {
        private readonly IPlayerCacheService _cache;
        private readonly ILogger<ScreenService> _logger;
        private readonly IWebHostEnvironment _env;

        public ScreenService(
            IPlayerCacheService cache,
            ILogger<ScreenService> logger,
            IWebHostEnvironment env)
        {
            _cache = cache;
            _logger = logger;
            _env = env;
        }

        public async Task<HomeDTOs?> GetHomeScreenAsync(int playerId, string sessionId, CancellationToken ct = default)
        {

            var (player, question) = await _cache.GetOrLoadLockedAsync(playerId, sessionId, ct);

            if (player == null)
            {
                //_logger.LogWarning("Player not found in cache. userId={UserId}", userId);
                return null;
            }
            if (player.SessionId == "denied")
                return new HomeDTOs() { AccessDenied = true };
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            var url = Path.Combine(_env.WebRootPath, "BulletinBoard", culture, "bb.html");

            var dt = File.GetLastWriteTimeUtc(url);
            // --- Karakterek számlálása egy passzban, null-safe ---
            int activeChars = 0;

            // ha a collection null lehet, kezeljük
            var charValues = player.Characters;
            if (charValues is not null)
            {
                foreach (var ch in charValues)
                {
                    if (ch is null) continue;

                    if (ch.EnergyPoints > 0) activeChars++;
                }
            }

            var nextLevelXp = RankRewards.List[player.Core.RankEnum].NextLevel;

            // 
            //  Place holder itt majd lesz db lekérés 
            // 
            var extendedInfo = new HomeExtendedInfo
            {
                LastInfo = dt,
                NextXp = nextLevelXp
            };

            var homeScreen = new HomeScreen
            {
                NavBarEnable = true,
                Team = new ScreenButtonEntity
                {
                    Enable = true,
                    FooterData1 = activeChars,
                    FooterData2 = RankRewards.List[player.Core.RankEnum].MaxCharacters
                },
                Question = new ScreenButtonEntity
                {
                    Enable = true,
                    //FooterData1 = 2 // TODO: később lekérdezésből
                },
                SoloGame = new ScreenButtonEntity
                {
                    Enable = true
                },
                VsGame = new ScreenButtonEntity
                {
                    Enable = activeChars > 2,
                    FooterData1 = 789 // TODO: később lekérdezésből
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
                    FooterData2 = 3  // TODO
                },
                Ranking = new ScreenButtonEntity
                {
                    Enable = false,
                    FooterData1 = 9 // TODO
                },
                Events = new ScreenButtonEntity
                {
                    Enable = false,     // ← korábban kétszer állítottad (true majd false)
                    FooterData1 = 99,   // TODO
                    FooterData2 = 8     // TODO
                },
                InfoBoard = new ScreenButtonEntity
                {
                    Enable = true,
                }

            };

            // FIGYELEM: ha nem akarod visszaadni a cache-ben lévő referenciát,
            // másold át külön DTO-ba a UserMainData-t.
            var userMain = new UserMainData();
            userMain.PlayerId = playerId;
            userMain.UserName = player.Core.DisplayName;
            userMain.TeamName = player.Core.TeamName;
            userMain.RankEnum = player.Core.RankEnum;
            userMain.XP = player.Core.XP;
            userMain.Credit = player.Core.Credit;
            userMain.Voucher = player.Core.Voucher;

            return new HomeDTOs
            {
                UserMainData = userMain,
                ExtendedInfo = extendedInfo,
                HomeScreen = homeScreen
            };
        }
        public async Task<TeamDtos?> GetTeamScreenDataAsync(int playerId, string sessionId, CancellationToken ct = default)
        {
            //sessionId = "Teszt";
            var (player, slot) = await _cache.GetOrLoadLockedAsync(playerId, sessionId, ct);

            if (player is null)
            {
                _logger.LogWarning("Player not found in cache. userId={UserId}", playerId);
                return null;
            }
            if (player.SessionId == "denied")
                return new TeamDtos() { AccessDenied = true };

            TeamMemberDto?[] teamMembers = new TeamMemberDto[9];
            CandidateDto?[] candidates = new CandidateDto[9];
            MembRemark[] membRemarks = new MembRemark[9];

            bool[] tempCharmask = new bool[9];
            bool[] tempAbleToHiremask = new bool[9];
            int numberOfCharacters = 0;
            int numberOfAbleToHire = 0;
            teamMembers[0] = null;
            candidates[0] = null;
            tempCharmask[0] = true;
            tempAbleToHiremask[0] = false;

            for (int i = 1; i < 9; i++)
            {
                var character = player.Characters[i - 1];
                if (character != null)
                {
                    int tempRank = player.Characters[i - 1] != null ? character.Rank : 0;
                    int nextRank = Math.Min(21, character.Rank + 1);
                    teamMembers[i] = new TeamMemberDto
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
                        MaintAttitude = AttitudeResolver(character.Attitude.Main, tempRank, RankConstants.maxLevels[0..4], RankConstants.startLevels[0..4], 0),
                        SecondAttitude = AttitudeResolver(character.Attitude.Secondary, tempRank, RankConstants.maxLevels[4..8], RankConstants.startLevels[4..8], character.DevPoints),
                        GenderAttitude = AttitudeResolver(character.Attitude.Gender, tempRank, RankConstants.maxLevels[8..12], RankConstants.startLevels[8..12], character.DevPoints)
                    };
                    membRemarks[i] = RemarkResolver(teamMembers[i]!, player.Core.DevPoint, player.Core.RankEnum);
                    teamMembers[i]!.Remark = membRemarks[i];
                    tempCharmask[i] = true;
                    numberOfCharacters++;
                }
                else
                {
                    teamMembers[i] = null;
                    tempCharmask[i] = false;
                }

                var candidate = player.CandidateCharacters[i - 1];
                if (candidate != null)
                {
                    candidates[i] = new CandidateDto
                    {
                        Name = candidate.Names,
                        PictureCode = candidate.PictureCodes,
                        CanBeHire = candidate.Names == null || candidate.PictureCodes == null ? false : true,
                        ExpirationTime = candidate.ExpirationTime
                    };
                }
                else
                {
                    candidates[i] = new CandidateDto
                    {
                        CanBeHire = false,
                    };

                }
                tempAbleToHiremask[i] = candidates[i].CanBeHire;

            }

            var teamInfo = new TeamExtendedInfo
            {
                Name = player.Core.TeamName,
                Level = player.Core.RankEnum,
                Xp = player.Core.XP,
                NextXp = RankRewards.List[player.Core.RankEnum].NextLevel,
                DevPoints = player.Core.DevPoint,
                TotalMembers = numberOfCharacters,
                MaxMembers = RankRewards.List[player.Core.RankEnum].MaxCharacters,
                AbleToHireMask = tempAbleToHiremask,
                Bonus = RankRewards.List[player.Core.RankEnum].WinBonus,
                Credits = player.Core.Credit,
                MembRemarks = membRemarks

            };

            var help = HelpResolver(player.Loadout?.HelpLevelsJson, teamInfo.Level, teamInfo.DevPoints);

            var rootBoxInfo = TeamRootBoxInfoResolver(teamInfo, help.CanDev);

            var teamDto = new TeamDtos
            {
                TeamInfo = teamInfo,
                TeamMembers = teamMembers,
                Candidates = candidates,
                CharCatMask = tempCharmask,
                Help = help,
                RootBoxInfo = rootBoxInfo
            };

            return teamDto;
        }
        public async Task<QuestionDtos?> GetQuestionScreenAsync(int playerId, string sessionId, CancellationToken ct = default)
        {
            //var sessionId = "Teszt";

            var (player, slot) = await _cache.GetOrLoadLockedAsync(playerId, sessionId, ct);
            if (player is null)
            {
                _logger.LogWarning("Player not found in cache. userId={UserId}", playerId);
                return null;
            }
            if (player.SessionId == "denied")
                return new QuestionDtos() { AccessDenied = true };
            // ----- Rank / slot kapacitások -----
            var level = player.Core.RankEnum; // invariáns tábla szerint létezik
            var rewards = RankRewards.List[level];
            var availableUserSlot = rewards.OwnQuestSlot;
            var availablePendingSlot = availableUserSlot >> 1;
            ///
            /// ----- Jelenlegi foglaltság -----
            ///
            var loadout = player.Loadout;
            var charmask = player.CharCatMask;

            var userSlotEnable = level > 0;

            var factorySlots = string.IsNullOrEmpty(loadout?.FactorySlotsJson)
                ? Array.Empty<int>()
                 : JsonSerializer.Deserialize<int[]>(loadout.FactorySlotsJson) ?? Array.Empty<int>();
            var chkOwnCount = factorySlots.Count(c => c == 17);

            var userSlotsDtoList = new List<UserSlot>();
            var pendingSlotDtoList = new List<PendingSlot>();

            foreach (var uq in slot.uSlots)
            {
                var answers = string.IsNullOrEmpty(uq.AnswersJson)
                ? Array.Empty<string>()
                 : JsonSerializer.Deserialize<string[]>(uq.AnswersJson) ?? Array.Empty<string>();

                userSlotsDtoList.Add(new UserSlot
                {
                    Question = uq?.Question ?? string.Empty,
                    Answer = answers,
                    Category = uq?.CategoryNo ?? 0,
                    NoOfUse = uq.Ask > 0 ? uq.Ask.ToString() : "N/A",
                    NofOfCorrect = uq.Ask > 0 ? uq.OkAnswer.ToString() : "N/A",
                    Ratio = uq.Ask > 40 ? $"{(Math.Truncate(uq.Ratio * 1000) / 10):0.0}%" : "N/A"
                });

            }
            foreach (var pq in slot.pSlots)
            {
                var answers = string.IsNullOrEmpty(pq.AnswersJson)
                 ? []
                 : JsonSerializer.Deserialize<string[]>(pq.AnswersJson) ?? [];

                pendingSlotDtoList.Add(new PendingSlot
                {
                    Question = pq?.Question ?? string.Empty,
                    Answer = answers,
                    Category = pq?.CategoryNo ?? 0,
                    Status = pq?.Status.ToString() ?? "None",
                    Remark = pq?.Remark,
                    SubmittedAt = pq?.SubmittedAt ?? DateTime.UtcNow
                });
            }
            var userSlotsDto = userSlotsDtoList.ToArray();
            var pendingSlotsDto = pendingSlotDtoList.ToArray();
            // Mozgathatóság/akciók feltételei

            var freeUserSlot = userSlotsDto.Take(10).Count(v => v.Category == 0);
            var freePendingSlot = pendingSlotsDto.Take(5).Count(v => v.Category == 0);
            var occupiedUserSlots = availableUserSlot - freeUserSlot;
            var occupiedPendingSlots = availablePendingSlot - freePendingSlot;


            var movablePendingCount = pendingSlotsDto.Take(5).Count(v => v is { Status: "Approved" });
            var rejectedPendingCount = pendingSlotsDto.Take(5).Count(v => v is { Status: "Rejected" });
            var isPendingCount = pendingSlotsDto.Take(5).Count(v => v is { Status: "Pending" });
            if (occupiedUserSlots < chkOwnCount) // ha a gyári közt van saját kérdés nem lehet belöle több mint a user slot
            {
                var j = 0;
                for (var i = 0; i < factorySlots.Length; i++)
                {
                    if (factorySlots[i] == 17) j++;
                    Console.WriteLine("------------------------------------------------------");
                    Console.WriteLine($"Slot:{i} Category:{factorySlots[i]} a j értéke:{j}");
                    factorySlots[i] = factorySlots[i] == 17 && j > occupiedUserSlots ? 0 : factorySlots[i];
                    Console.WriteLine($"Slot:{i} Category:{factorySlots[i]} a j értéke:{j}");
                }
                await _cache.UpdatePartialLoadoutLockedAsync(
                    playerId,
                    sessionId,
                    new PlayerLoadout
                    {
                        FactorySlotsJson = JsonSerializer.Serialize(factorySlots)
                    },
                    ct);
            }


            var extendedInfo = new QuestionExtendedInfo
            {
                AvailablePendingSlot = availablePendingSlot,
                AvailableUserSlot = availableUserSlot,
                FreeUserSlot = freeUserSlot,
                FreePendingSlot = freePendingSlot,
                OccupiedUserSlot = occupiedUserSlots,
                OccupiedPendingSlot = occupiedPendingSlots,
                HandlePendingSlot = movablePendingCount + rejectedPendingCount,
                UserSlotEnable = userSlotEnable,
                NoFownQuestion = chkOwnCount,
                CharCatMask = charmask
            };
            /*
            var buttons = new QuestionButtons
            {
                btnFactorySave = true,
                btnUsrSave = userSlotEnable,
                btnUsrDelete = userSlotEnable,
                btnPendigMoveUsr = userSlotEnable && movablePendingCount > 0,
                btnPendingDelete = userSlotEnable && (movablePendingCount + rejectedPendingCount) > 0,
               
            };*/

            return new QuestionDtos
            {
                FactorySlots = factorySlots,
                Userlots = userSlotsDto,
                PendingSlots = pendingSlotsDto,
                ExtendedInfo = extendedInfo
                //QuestionButtons = buttons
            };
        }
        public async Task<SoloGameDtos?> GetSoloGameScreenAsync(int playerId, string sessionId, CancellationToken ct = default)
        {
            //sessionId = "Teszt";
            var (player, slot) = await _cache.GetOrLoadLockedAsync(playerId, sessionId, ct);

            if (player is null)
            {
                _logger.LogWarning("Player not found in cache. userId={UserId}", playerId);
                return null;
            }
            if (player.SessionId == "denied")
                return new SoloGameDtos() { AccessDenied = true };
            var mask = player.CharCatMask;
            var results = new SoloResults
            {
                OrientResults = GetOriResultFromCache(player.OrientStats),
                CategoryResults = GetCatResultFromCache(player.CategoryStats)

            };
            var enables = new SoloEnables
            {
                EnaCampaign = false,
                EnaCategory = mask.AsSpan().Contains(true),
                EnaOrient = mask.AsSpan().Contains(true),
                EnaOri = mask,
                EnaCat = mask.Concat(mask).ToArray(),
            };


            return new SoloGameDtos
            {
                Mask = mask,
                Results = results,
                Enables = enables
            };

        }
        /// <summary>
        /// Itt vanna az osztály privát helperei
        /// </summary>
        private static ResultDto[] GetCatResultFromCache(List<PlayerCategoryStat> data)
        {
            int ix;
            var result = new ResultDto[data.Count + 1];
            result[0] = new ResultDto { Points = 0, Time = 0.0 };
            foreach (var d in data)
            {
                ix = Math.Min((int)d.CategoryId, 16);
                result[ix] = new ResultDto
                {
                    Points = d.HighScore,
                    Time = d.HighScoreTime
                };
                result[0].Points = +d.HighScore;
                result[0].Time = +d.HighScoreTime;
            }
            return result;
        }
        private static ResultDto[] GetOriResultFromCache(List<PlayerOrientStat> data)
        {
            int ix;
            var result = new ResultDto[data.Count + 1];
            result[0] = new ResultDto { Points = 0, Time = 0.0 };
            foreach (var d in data)
            {
                ix = Math.Min((int)d.OrientId, 8);
                result[ix] = new ResultDto
                {
                    Points = d.HighScore,
                    Time = d.HighScoreTime
                };
                result[0].Points = +d.HighScore;
                result[0].Time = +d.HighScoreTime;
            }
            return result;
        }
        private static AttidtudeDto AttitudeResolver(AttitudeBranch attitude, int rank, int[] maxLevels, int[] startLevels, int devPoints)
        {
            var skill = SkillResolver(attitude.Level, rank, maxLevels, startLevels);
            return new AttidtudeDto
            {
                Category = attitude.CatNo.Take(4).Select(x => (byte)x).ToArray(),
                Skill = skill,
                CanDev = skill.Any(x => x.SkillCanDev) && devPoints > 0
            };
        }
        private static SkillPartial[] SkillResolver(int[] data, int mainActLevel, int[] constMaxLev, int[] constStartLev)
        {
            var sp = new SkillPartial[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                sp[i] = SkillPartialResolver(data[i], mainActLevel, constMaxLev[i], constStartLev[i] - 1);
            }
            return sp;
        }
        private static SkillPartial SkillPartialResolver(int currentLevel, int actualRank, int maxLevel, int startmodifier)
        {
            int maxmodify = Math.Max(0, actualRank - startmodifier);
            int maxlevel = Math.Min(maxLevel, maxmodify);
            maxlevel = Math.Max(0, maxlevel);
            return new SkillPartial
            {
                LvlCurrent = (byte)currentLevel,
                LvlCurMax = (byte)maxlevel,
                LvlOvrMax = (byte)maxLevel,
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
                IsRecruitEnable = freePositions > 0 && ableToHire > 0,
                IsMemberEnable = info.TotalMembers > 0,
                TeamOpRequired = retire + fire + promote + heal + help,
                MemberOpRequired = info.MembRemarks.Count(x => x == MembRemark.Develop),
                FreePositions = Math.Min(freePositions, ableToHire),
                AbleToHire = ableToHire
            };
        }
    }
}
