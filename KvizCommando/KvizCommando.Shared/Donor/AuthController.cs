using KvizCommando.Server.Data;
using KvizCommando.Server.Extensions;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.PlayerDataHandling;
using KvizCommando.Server.Services.Security;
using KvizCommando.Shared.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace KvizCommando.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ISessionService _sessionService;
        private readonly IPlayerCacheService _playerCacheService;
        private readonly IStringLocalizer<AuthController> _localizer;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly IPlayerJsonService _jsonService;

        public AuthController(
            AppDbContext db,
            ISessionService sessionService,
            IPlayerCacheService playerCacheService,
            IStringLocalizer<AuthController> localizer,
            IConfiguration config,
            IWebHostEnvironment env,
            IPlayerJsonService jsonService)
        {
            _db = db;
            _sessionService = sessionService;
            _playerCacheService = playerCacheService;
            _localizer = localizer;
            _config = config;
            _env = env;
            _jsonService = jsonService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
             var user = await _db.Users
                 .FirstOrDefaultAsync(u => u.Name == request.Username && u.PasswordHash == request.Password);

             if (user is null)
                return Unauthorized(_localizer["error.LoginFail"].Value);
            /*
            await Task.Delay(2000);
            User user = new User()
            {
                Id = 29,
                Name = request.Username,
                PasswordHash = request.Password,
                Email = "kamu@kamu.ku"
            };
            */
            //await Task.Delay(4000);

            // --- JWT beállítások konfigurációból ---
            var issuer = _config["Auth:Issuer"] ?? "KvizCommando";
            var audience = _config["Auth:Audience"]; // opcionális
            var keyStr = _config["Auth:SigningKey"] ?? "Dev_Fallback_Key_ChangeMe_OnlyLocal";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(12),
                Issuer = issuer,
                Audience = string.IsNullOrWhiteSpace(audience) ? null : audience,
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            var tokenString = handler.WriteToken(token);

            // --- Cookie kiadása SPA ↔ API-hoz ---
            // Cross-origin esetén kell: SameSite=None; Secure; HttpOnly
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,                 // böngészők SameSite=None-hoz megkövetelik
                SameSite = SameSiteMode.None,  // hogy a böngésző elküldje cross-site kérésekben is
                Path = "/",                    // teljes appra érvényes
                Expires = DateTimeOffset.UtcNow.AddHours(12),
                IsEssential = true
            };
            // Ha külön domainre akarod beállítani, ide teheted:
            // cookieOptions.Domain = _config["Auth:CookieDomain"];

            Response.Cookies.Append("authToken", tokenString, cookieOptions);

            // Session + cache frissítés (változatlan)
            var sessionKey = _sessionService.GenerateAndStoreSessionKey(user.Id.ToString());
            int playerId = user.Id;
            await _playerCacheService.RefreshAsync(playerId);

            return Ok(); // payloadot most nem változtatok
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (_db.Users.Any(u => u.Name == request.UserName))
            {

                return BadRequest(_localizer["Error.UsernameExists"].Value);
            }

            if (_db.Users.Any(u => u.Email == request.Email))
            {

                return BadRequest(_localizer["Error.EmailExists"].Value);

            }

           
            string hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(request.Password)));

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var newUser = new User
                {
                    Name = request.UserName,
                    Email = request.Email,
                    PasswordHash = hash,
                    LastSeen = DateTime.UtcNow,
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "?"
                };

                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();

                var playerJson = new PlayerData
                {
                    PlayerId = newUser.Id,
                    UserName = newUser.Name,

                };
                playerJson.UserMainData = new UserMainData
                {
                    UserName = playerJson.UserName,
                    PlayerId = playerJson.PlayerId,
                    TeamName = playerJson.UserName + _localizer["OpTeamGen"] // Példa csapatnév generálásra
                };
                Console.WriteLine($"[RegisterController] Creating player JSON for user: {playerJson.UserName} with ID: {playerJson.PlayerId}");
                await _jsonService.CreateAsync(playerJson);

                await transaction.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine(ex.ToString());
                Console.WriteLine(_localizer["Error.DataBase"].Value);
                return BadRequest(_localizer["Error.DataBase"].Value);
                //return StatusCode(500, "Hiba történt a regisztráció során: " + ex.Message);
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var userId = User.GetUserId();
            // 1. Töröljük a cookie-t
            Response.Cookies.Delete("authToken");

            // 2. Userhez kötött cache törlése
            
            if (userId != null)
            {
                _playerCacheService.Remove(userId.Value);
            }

            return Ok();
        }

    }

   
}

/*
using KvizCommando.Server.Data;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.Security;
using KvizCommando.Shared.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace KvizCommando.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ISessionService _sessionService;
        private readonly IPlayerCacheService _playerCacheService;
        private readonly IStringLocalizer<AuthController> _localizer;
        public AuthController(
            AppDbContext db,
            ISessionService sessionService,

            IPlayerCacheService playerCacheService,
            IStringLocalizer<AuthController> localizer
            )
        {
            _db = db;
            _sessionService = sessionService;
            _playerCacheService = playerCacheService;
            _localizer = localizer;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            // int playerId = 22; // bypass
          
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Name == request.Username && u.PasswordHash == request.Password);

            if (user is null)
                return Unauthorized(_localizer["error.LoginFail"].Value);
           
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("EgyszerEgyGyermekNemKerteletValobanHossz123");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            //new Claim(ClaimTypes.NameIdentifier, playerId.ToString()),
            //new Claim(ClaimTypes.Name, request.Username),

        }),
                Expires = DateTime.UtcNow.AddHours(12),
                Issuer = "KvizCommando",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            Response.Cookies.Append("authToken", tokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(12)
            });




            //var sessionKey = _sessionService.GenerateAndStoreSessionKey(playerId.ToString());
             var sessionKey = _sessionService.GenerateAndStoreSessionKey(user.Id.ToString());
            int playerId = user.Id;           
            await _playerCacheService.RefreshAsync(playerId);

            //var currentCulture = HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()?.RequestCulture.Culture.Name
            //            ?? System.Globalization.CultureInfo.CurrentCulture.Name;
            //Console.WriteLine($"Aktuális kérés kultúrája: {currentCulture}");

            // ⬇️ Most már visszaadjuk az alap user adatokat
            return Ok(
             //   new LoginResult
            //{
                //UserId = playerId,
                //Username = request.Username,
                //UserId = user.Id,
                //Username = user.Name,
                //SessionKey = sessionKey

            //}
            );
        }

    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }


} 
 */