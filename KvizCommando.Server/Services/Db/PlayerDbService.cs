using KvizCommando.Server.Domain.Entities.Compliance;
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Logging;
using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Server.Models;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace KvizCommando.Server.Services.Db
{
    public sealed class PlayerDbService : IPlayerDbService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PlayerDbService> _logger;
        private readonly ILookupNormalizer _normalizer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly IAuditLogger _audit;
        public PlayerDbService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            ILogger<PlayerDbService> logger,
            ILookupNormalizer normalizer,
            IHttpContextAccessor httpcontextaccessor,
            IConfiguration config,
            IAuditLogger audit)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
            _normalizer = normalizer;
            _httpContextAccessor = httpcontextaccessor;
            _config = config;
            _audit = audit;
        }

        /// <inheritdoc />
        public async Task<CachedPlayer?> LoadPlayerFromDbAsync(
            int playerId,
            string sessionId,
            CancellationToken ct)
        {
            var player = await _db.Set<Player>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PlayerId == playerId, ct);

            if (player is null)
                return null;

            var loadout = await _db.Set<PlayerLoadout>()
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.PlayerId == playerId, ct);

            var characters = await _db.Set<PlayerCharacter>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.PlayerId == playerId, ct);


            var askStats = await _db.Set<PlayerAskStats>()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.PlayerId == playerId, ct);

            var categoryStats = await _db.Set<PlayerCategoryStat>()
                .AsNoTracking()
                .Where(cs => cs.PlayerId == playerId)
                .ToListAsync(ct);
            var orientStats = await _db.Set<PlayerOrientStat>()
                .AsNoTracking()
                .Where(cs => cs.PlayerId == playerId)
                .ToListAsync(ct);
            var teamStats = await _db.Set<TeamStatistic>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    stat => stat.PlayerId == playerId,
                    ct) ??
                new TeamStatistic { PlayerId = playerId };

            teamStats.RankedPlacements =
                JsonSerializer.Deserialize<RankedPlacementStatistic>(
                    teamStats.RankedPlacementsJson) ??
                new RankedPlacementStatistic();

            var tempChars = characters.CharactersJson.ConvertToArray<CharachterSlot>();
            var tempCand = characters.CandidatesJson.ConvertToArray<RecruitSlot>();


            bool[] tempCharMask = [.. tempChars.Select(x => x != null)];

            return new CachedPlayer
            {
                Core = player,
                Loadout = loadout ?? new PlayerLoadout
                {
                    PlayerId = playerId,
                    FactorySlotsJson = "[]",
                    UserSlotsJson = "[]",
                    PendingSlotsJson = "[]",
                    UpdatedUtc = DateTime.UtcNow
                },
                Characters = tempChars,
                CandidateCharacters = tempCand,
                CharCatMask = tempCharMask,
                AskStats = askStats ?? new PlayerAskStats
                {
                    PlayerId = playerId,
                    TotalQuestionsAsked = 0,
                    TotalAskPointsEarned = 0
                },
                CategoryStats = categoryStats,
                OrientStats = orientStats,
                TeamStats = teamStats,
                SessionId = sessionId
            };

        }

        /// <inheritdoc />
        public async Task<bool> SavePlayerToDbAsync(
            CachedPlayer player,
            DirtyFlags flags,
            int playerId,
            CancellationToken ct)
        {
            try
            {
                // Játékos törzsadatai
                if ((flags & DirtyFlags.Core) != 0)
                    _db.Update(player.Core);

                // Kérdéslista
                if ((flags & DirtyFlags.Loadout) != 0)
                {
                    player.Loadout.UpdatedUtc = DateTime.UtcNow;
                    _db.Update(player.Loadout);
                }

                // Karakterek és jelöltek
                if ((flags & DirtyFlags.Characters) != 0)
                {
                    var serializedChars = System.Text.Json.JsonSerializer.Serialize(player.Characters);
                    var serializedCandidates = System.Text.Json.JsonSerializer.Serialize(player.CandidateCharacters);
                    var dbChars = await _db.Set<PlayerCharacter>()
                        .FirstOrDefaultAsync(c => c.PlayerId == playerId, ct);

                    if (dbChars is null)
                    {
                        dbChars = new PlayerCharacter
                        {
                            PlayerId = playerId,
                            CharactersJson = serializedChars,
                            CandidatesJson = serializedCandidates,
                        };
                        await _db.AddAsync(dbChars, ct);
                    }
                    else
                    {
                        dbChars.CharactersJson = serializedChars;
                        dbChars.CandidatesJson = serializedCandidates;
                        _db.Update(dbChars);
                    }
                }

                // Kérdésstatisztika
                if ((flags & DirtyFlags.AskStats) != 0)
                    _db.Update(player.AskStats);

                // Kategóriastatisztika
                if ((flags & DirtyFlags.CategoryStats) != 0)
                {
                    foreach (var stat in player.CategoryStats)
                        _db.Update(stat);
                }

                if ((flags & DirtyFlags.OrientStats) != 0)
                {
                    foreach (var stat in player.OrientStats)
                        _db.Update(stat);
                }

                if ((flags & DirtyFlags.TeamStats) != 0)
                {
                    player.TeamStats.RankedPlacementsJson =
                        JsonSerializer.Serialize(
                            player.TeamStats.RankedPlacements);
                    _db.Update(player.TeamStats);
                }


                await _db.SaveChangesAsync(ct);
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <inheritdoc />
        public async Task<(ApplicationUser?, string?, int?)> LoadCheckinDataFromDbAsync(
            string userId,
            CancellationToken ct)
        {
            var user = await _userManager.Users
                .Where(u => u.Id == userId)
                .SingleAsync(ct);
            var playerId = await _db.Players
                .Where(p => p.UserId == userId)
                .Select(p => p.PlayerId)
                .FirstOrDefaultAsync(ct);


            var lastAcceptedVersion = await _db.Set<TermsConsent>()
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.AcceptedAtUtc)
                .Select(x => x.TermsVersion)
                .FirstOrDefaultAsync(ct);

            return (user, lastAcceptedVersion, playerId);
        }
        /// <inheritdoc />
        public async Task<(IReadOnlyList<string>, bool success)> SaveDisplayNameToDbAsync(ApplicationUser user, string displayName, CancellationToken ct)
        {
            var errorKeys = new List<string>();
            // A keresési és egyediségi ellenőrzéshez a nyilvános név normalizált párját is tárolni kell.
            user.DisplayName = displayName;
            user.NormalizedDisplayName = _normalizer.NormalizeName(displayName);
            user.PreferredLocale = CultureInfo.CurrentUICulture.Name;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                errorKeys.AddRange(updateResult.Errors.Select(e => e.Code));
                return (errorKeys, false);
            }
            return (Array.Empty<string>(), true);
        }

        /// <inheritdoc />
        public async Task<bool> SaveTermsToDbAsync(
            ApplicationUser user,
            string acceptedTerms,
            string currentTerms,
            DateTime acceptedAt,
            CancellationToken ct)
        {
            var exists = await _db.Set<TermsConsent>()
                    .AnyAsync(x => x.UserId == user.Id && x.TermsVersion == currentTerms, ct);

            if (!exists)
            {
                // A user-agent és IP csak kulcsos hash formájában kerül az auditbejegyzésbe.
                var http = _httpContextAccessor.HttpContext;
                var ua = http?.Request?.Headers["User-Agent"].ToString();
                var ip = http?.Connection?.RemoteIpAddress?.ToString();

                byte[]? secretKey = null;
                var secretB64 = _config["AuditHash:Secret"];
                if (!string.IsNullOrWhiteSpace(secretB64))
                {
                    try
                    {
                        secretKey = Convert.FromBase64String(secretB64);
                    }
                    catch
                    {
                        // Érvénytelen auditkulcs esetén a hálózati metaadatok hash nélkül maradnak.
                        secretKey = null;
                    }
                }
                // Az adatbázis és a claim ugyanazt az elfogadási időpontot kapja.


                _db.Add(new TermsConsent
                {
                    UserId = user.Id,
                    TermsVersion = currentTerms,
                    AcceptedAtUtc = acceptedAt,
                    UserAgentHash = HmacOrNull(secretKey, ua),
                    IpHash = HmacOrNull(secretKey, ip)
                });

                await _db.SaveChangesAsync(ct);
                await _audit.LogAsync(
                    new AuditEntry(
                        AuditEvents.TERMS_ACCEPTED,
                        AuditOutcome.Succeeded,
                        user.Id,
                        user.Id,
                        ip,
                        http?.TraceIdentifier,
                        new AuditDetails(DocumentVersion: currentTerms)));

            }

            return true;
        }
        /// <inheritdoc />
        public async Task<int> CreatePlayerToDbAsync(
            string userId,
            string displayname,
            string teamname,
            int startingCredit,
            CancellationToken ct)
        {
            var has = await _db.Set<Player>()
                .AsNoTracking()
                .AnyAsync(p => p.UserId == userId, ct);

            if (has) return 0;

            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var now = DateTime.UtcNow;

                var player = new Player
                {
                    UserId = userId,
                    RankEnum = 0,
                    XP = 0,
                    Voucher = 0,
                    Credit = startingCredit,
                    DisplayName = displayname,
                    TeamName = teamname,
                    NormalizedTeamName =
                        _normalizer.NormalizeName(teamname) ??
                        teamname.ToUpperInvariant(),
                    CaptainAvatar = "0",
                    CreatedUtc = now,
                    UpdatedUtc = now
                };
                _db.Add(player);
                await _db.SaveChangesAsync(ct);

                _db.Add(new PlayerLoadout
                {
                    PlayerId = player.PlayerId,
                    FactorySlotsJson = "[" + string.Join(",", Enumerable.Repeat(0, 12)) + "]",
                    UserSlotsJson = "[]",
                    PendingSlotsJson = "[]",
                    HelpLevelsJson = "[" + string.Join(",", Enumerable.Repeat(0, 8)) + "]",
                    UpdatedUtc = now
                });

                _db.Add(new PlayerCharacter
                {
                    PlayerId = player.PlayerId,
                    CharactersJson = "[null,null,null,null,null,null,null,null]",
                    CandidatesJson = "[null,null,null,null,null,null,null,null]",
                    UpdatedUtc = now
                });



                _db.Add(new PlayerAskStats
                {
                    PlayerId = player.PlayerId,
                    TotalQuestionsAsked = 0,
                    TotalAskPointsEarned = 0
                });

                _db.Add(new TeamStatistic
                {
                    PlayerId = player.PlayerId
                });

                for (short categoryId = 1; categoryId <= 16; categoryId++)
                {
                    _db.Add(new PlayerCategoryStat
                    {
                        PlayerId = player.PlayerId,
                        CategoryId = categoryId,
                        Answered = 0,
                        Correct = 0,
                        HighScore = 0,
                        HighScoreTime = 0.0
                    });
                }
                for (short oreintId = 1; oreintId <= 8; oreintId++)
                {
                    _db.Add(new PlayerOrientStat
                    {
                        PlayerId = player.PlayerId,
                        OrientId = oreintId,
                        HighScore = 0,
                        HighScoreTime = 0.0
                    });
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return player.PlayerId;
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(
                    ex,
                    "Player creation failed. UserId={UserId}",
                    userId);
                throw;
            }

        }

        /// <inheritdoc />
        public Task<bool> IsNormalizedTeamNameTakenAsync(
            string normalizedTeamName,
            int excludedPlayerId,
            CancellationToken ct = default) =>
            _db.Set<Player>()
                .AsNoTracking()
                .AnyAsync(player =>
                    player.PlayerId != excludedPlayerId &&
                    player.NormalizedTeamName == normalizedTeamName,
                    ct);

        /// <inheritdoc />
        public async Task<string> SuggestAsync(string? rawName, CancellationToken ct = default)
        {
            var raw = (rawName ?? string.Empty).Trim();

            // A megjelenítési alap megőrzi a kis- és nagybetűket, de csak az engedélyezett ASCII-karaktereket tartja meg.
            var baseRaw = ToAsciiBase(raw);
            if (string.IsNullOrEmpty(baseRaw)) baseRaw = "Player";

            var baseNorm = baseRaw.ToUpperInvariant();

            // Szabad névnél a megjelenítési alak változatlan marad.
            var exactTaken = await _db.Users.AnyAsync(u => u.NormalizedDisplayName == baseNorm, ct);
            if (!exactTaken) return baseRaw;

            // Foglalt névnél a normalizált mező legnagyobb numerikus utótagja határozza meg a következő értéket.
            var taken = await _db.Users
                .Where(u => u.NormalizedDisplayName.StartsWith(baseNorm))
                .Select(u => u.NormalizedDisplayName)
                .ToListAsync(ct);

            var prefixLen = baseNorm.Length;
            var maxSuffix = 0;
            foreach (var dn in taken)
            {
                if (dn.Length <= prefixLen) continue;
                var tail = dn.Substring(prefixLen);
                if (tail.All(char.IsDigit) && int.TryParse(tail, out var n) && n > maxSuffix)
                    maxSuffix = n;
            }

            var next = maxSuffix + 1;
            var digits = next.ToString();

            // Az utótag számára szükséges helyet a név maximális hosszából kell fenntartani.
            var allowed = Math.Max(0, 20 - digits.Length);
            var cutRaw = baseRaw.Length > allowed ? baseRaw[..allowed] : baseRaw;

            return cutRaw + digits;
        }

        // A FormD bontás után a kombináló jelek eltávolítása ad stabil ASCII névalapot.
        private static readonly Regex _combiningMarks = new(@"\p{Mn}+", RegexOptions.Compiled);
        private static string ToAsciiBase(string s)
        {
            var decomp = s.Normalize(NormalizationForm.FormD);
            var noMarks = _combiningMarks.Replace(decomp, "");
            var sb = new StringBuilder(20);
            foreach (var ch in noMarks)
            {
                if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
                {
                    sb.Append(ch);
                    if (sb.Length == 20) break;
                }
            }
            return sb.ToString();
        }
        private static byte[]? HmacOrNull(byte[]? key, string? value)
        {
            if (key == null || string.IsNullOrEmpty(value)) return null;
            using var h = new HMACSHA256(key);
            return h.ComputeHash(Encoding.UTF8.GetBytes(value));
        }
    }

}
