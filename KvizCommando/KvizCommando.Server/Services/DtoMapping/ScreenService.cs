using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.User;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Globalization;

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

            var (player, _) = await _cache.GetOrLoadLockedAsync(playerId, sessionId, ct);

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

        public async Task<SoloGameDtos?> GetSoloGameScreenAsync(int playerId, string sessionId, CancellationToken ct = default)
        {
            //sessionId = "Teszt";
            var (player, _) = await _cache.GetOrLoadLockedAsync(playerId, sessionId, ct);

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
                    TimeStr = tempTime > 99.9  ? "+99.9" : tempTime.ToString("0.0", CultureInfo.InvariantCulture)
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

    }
}



/*
 
  public async Task<TeamDtos?> GetTeamScreenDataAsync2(int playerId, string sessionId, CancellationToken ct = default)
        {

            var (player, _) = await _cache.GetOrLoadLockedAsync(playerId, sessionId, ct);

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
                        CanBeHire = candidate.Names != null && candidate.PictureCodes != null,
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
                tempAbleToHiremask[i] = candidates[i]!.CanBeHire;

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

            var help = HelpResolver(player.Loadout.HelpLevelsJson, teamInfo.Level, teamInfo.DevPoints);

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



 
 */