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
                CaptainAvatar = player.Core.CaptainAvatar,
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
    }
}
