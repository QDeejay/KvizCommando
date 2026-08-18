using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.VsGame;
using KvizCommando.Server.Services.VsGame.Matchmaking;
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
            // --- Karakterek számlálása egy passzban, null-safe ---
            int characterCount = 0;
            int activeChars = 0;

            // ha a collection null lehet, kezeljük
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
                        battleReadyMemberCount >= VsBattleClassificationRules.RequiredBattleReadyCharacters &&
                        player.Core.Credit >= VsBattleClassificationRules.RequiredCreditBalance,
                    BattleReadyCharacterCount =  battleReadyMemberCount,
                    RequiredBattleReadyCharacterCount = VsBattleClassificationRules.RequiredBattleReadyCharacters,
                    CreditBalance = player.Core.Credit,
                    RequiredCreditBalance = VsBattleClassificationRules.RequiredCreditBalance,
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
