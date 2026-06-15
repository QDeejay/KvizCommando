using KvizCommando.Server.Domain.Entities.Compliance;
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Security;
using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Server.Models;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Utilities.Recruit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
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
        public PlayerDbService(
            ApplicationDbContext db, 
            UserManager<ApplicationUser> userManager,
            ILogger<PlayerDbService> logger,
            ILookupNormalizer normalizer,
            IHttpContextAccessor httpcontextaccessor,
            IConfiguration config) 
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
            _normalizer = normalizer;
            _httpContextAccessor = httpcontextaccessor;
            _config = config;
        }

        /// <summary>
        /// Teljes player csomag betöltése az adatbázisból
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="sessionId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
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


            CharachterSlot?[] tempChars = characters is null
                        ? new CharachterSlot?[8]
                         : System.Text.Json.JsonSerializer
                        .Deserialize<CharachterSlot?[]>(characters.CharactersJson)!;
            RecruitSlot?[] tempCandidates = characters is null
                        ? new RecruitSlot?[8]
                         : System.Text.Json.JsonSerializer
                        .Deserialize<RecruitSlot?[]>(characters.CandidatesJson)!;

            bool[] tempCharMask = tempChars.Select(x => x != null).ToArray();
           
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
                CharCatMask = tempCharMask,
                AskStats = askStats ?? new PlayerAskStats
                {
                    PlayerId = playerId,
                    TotalQuestionsAsked = 0,
                    TotalAskPointsEarned = 0
                },
                CategoryStats = categoryStats,
                OrientStats = orientStats,
                SessionId = sessionId
            };

        }

        /// <summary>
        /// player adatok mentése a dirty flag alapján, csak azt mentjük ami változott
        /// </summary>
        /// <param name="player"></param>
        /// <param name="flags"></param>
        /// <param name="playerId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<bool> SavePlayerToDbAsync(
            CachedPlayer player,
            DirtyFlags flags,
            int playerId,
            CancellationToken ct)
        {
            try 
            {
                // --- Core Player ---
                if ((flags & DirtyFlags.Core) != 0)
                    _db.Update(player.Core);

                // --- Loadout ---
                if ((flags & DirtyFlags.Loadout) != 0)
                {
                    player.Loadout.UpdatedUtc = DateTime.UtcNow;
                    _db.Update(player.Loadout);
                }

                // --- Characters ---
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

                // --- AskStats ---
                if ((flags & DirtyFlags.AskStats) != 0)
                    _db.Update(player.AskStats);

                // --- CategoryStats ---
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


                await _db.SaveChangesAsync(ct);
                return false;
            }
            catch 
            { 
                return false;
            }
            finally { }
            
        }

        /// <summary>
        /// Check in folyamán lekérjük hogy van e display név és hogy Terms elfogadása up-to date e
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<(ApplicationUser?, string?, int?)> LoadCheckinDataFromDbAsync(
            string userId,
            CancellationToken ct)
        {
            var user = await _userManager.Users
                .Where(u => u.Id == userId)
                //.Select(u => new { u.DisplayName })
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
        /// <summary>
        /// Display név elmentése
        /// </summary>
        /// <param name="user"></param>
        /// <param name="displayName"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<(IReadOnlyList<string>, bool success)> SaveDisplayNameToDbAsync(ApplicationUser user, string displayName, CancellationToken ct)
        {
            var errorKeys = new List<string>();
            // Beállítjuk a display nevet és a normalizált párját is.
            user.DisplayName = displayName;
            user.NormalizedDisplayName = _normalizer.NormalizeName(displayName);
            user.PreferredLocale = CultureInfo.CurrentUICulture.Name;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                // az Identity a saját kódjait adja – továbbítjuk
                 errorKeys.AddRange(updateResult.Errors.Select(e => e.Code));
                return (errorKeys, false);
            }
            return (Array.Empty<string>(), true);
        }

        /// <summary>
        /// Terms elfogadás frissitése a táblában
        /// </summary>
        /// <param name="user"></param>
        /// <param name="acceptedTerms"></param>
        /// <param name="currentTerms"></param>
        /// <param name="acceptedAt"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
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
                // --- UA/IP + SECRET beolvasása ---
                var http = _httpContextAccessor.HttpContext;
                var ua = http?.Request?.Headers["User-Agent"].ToString();
                var ip = http?.Connection?.RemoteIpAddress?.ToString();

                byte[]? secretKey = null;
                var secretB64 = _config["AuditHash:Secret"];
                if (!string.IsNullOrWhiteSpace(secretB64))
                {
                    try { secretKey = Convert.FromBase64String(secretB64); } catch { /* marad null */ }
                }
                // --- időbélyeg egységesen a DB és a claim számára ---
                

                _db.Add(new TermsConsent
                {
                    UserId = user.Id,
                    TermsVersion = currentTerms,
                    AcceptedAtUtc = acceptedAt,
                    UserAgentHash = HmacOrNull(secretKey, ua),
                    IpHash = HmacOrNull(secretKey, ip)
                });

                await _db.SaveChangesAsync(ct);
                _logger.LogInformation("Terms accepted. UserId={UserId}, Version={Version}", user.Id, currentTerms);

            }

            return true;
        }
        /// <summary>
        /// Játékos adatok létrehozás, az első terms elfogadása után
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="displayname"></param>
        /// <param name="teamname"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task CreatePlayerToDbAsync(
            string userId, 
            string displayname, 
            string teamname, 
            CancellationToken ct)
        {
            var has = await _db.Set<Player>()
                .AsNoTracking()
                .AnyAsync(p => p.UserId == userId, ct);

            if (has) return;

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
                    Credit = 0,
                    DisplayName = displayname,
                    TeamName = teamname,
                    CreatedUtc = now,
                    UpdatedUtc = now
                };
                _db.Add(player);
                await _db.SaveChangesAsync(ct);

                _db.Add(new PlayerLoadout
                {
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
            }
            catch (DbUpdateException ex)
            {
                _logger.LogDebug(ex, "EnsurePlayerExists: concurrent insert ignored. UserId={UserId}", userId);
                await tx.RollbackAsync(ct);
            }

        }

        /// <summary>
        /// Facebook-os bejelentkezás után javaslunk display nevet a kereszt név alapján
        /// </summary>
        /// <param name="rawName"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<string> SuggestAsync(string? rawName, CancellationToken ct = default)
        {
            var raw = (rawName ?? string.Empty).Trim();

            // 1) Vizuális alap: ékezetek le (Regex \p{Mn}), csak [A-Za-z0-9], max 20, EREDETI case megtartva
            var baseRaw = ToAsciiBase(raw);
            if (string.IsNullOrEmpty(baseRaw)) baseRaw = "Player";

            // 2) Normalizált (DB) alap: csupa nagy
            var baseNorm = baseRaw.ToUpperInvariant();

            // 3) Ha pontosan szabad → a vizuális alapot adjuk (nem csupa nagy!)
            var exactTaken = await _db.Users.AnyAsync(u => u.NormalizedDisplayName == baseNorm, ct);
            if (!exactTaken) return baseRaw;

            // 4) Foglalt: keressük a max numerikus suffixet a NORMALIZÁLT mezőn
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

            // 5) 20-as limit tartása a VIZUÁLIS javaslatnál is
            var allowed = Math.Max(0, 20 - digits.Length);
            var cutRaw = baseRaw.Length > allowed ? baseRaw[..allowed] : baseRaw;

            return cutRaw + digits;
        }

        /// <summary>
        /// Helperek a db szervizhez
        /// Diakritikák levágása Regex-szel: FormD bontás + \p{Mn} (combining marks) eltávolítása
        /// </summary>
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
