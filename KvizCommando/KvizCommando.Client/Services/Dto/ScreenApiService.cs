using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Models.Dtos;
using System.Net.Http.Json;

namespace KvizCommando.Client.Services.Dto
{
    public sealed class ScreenApiService : IScreenApiService
    {
        private readonly HttpClient _http;
        private readonly SessionService _sessionCache;
        private const string SCREEN_ROUTE = "api/screen";
        public ScreenApiService(HttpClient http, SessionService sessioncache)
        {
            _http = http;
            _sessionCache = sessioncache;
        }
        public async Task<HomeDTOs?> GetHomeScreenAsync(CancellationToken ct = default)
        {
            var SessionId = _sessionCache.SessionId;
            var dto = await _http.GetFromJsonAsync<HomeDTOs>($"{SCREEN_ROUTE}/home?sessionId={SessionId}");
            return dto;

        }
        public async Task<QuestionDtos?> GetQuestionAsync(CancellationToken ct = default)
        {
            var SessionId = _sessionCache.SessionId;
            var dto = await _http.GetFromJsonAsync<QuestionDtos>($"{SCREEN_ROUTE}/question?sessionId={SessionId}");
            return dto;
        }
        public async Task<TeamDtos?> GetTeamAsync(CancellationToken ct = default)
        {
            var SessionId = _sessionCache.SessionId;
            var dto = await _http.GetFromJsonAsync<TeamDtos>($"{SCREEN_ROUTE}/team?sessionId={SessionId}");
            return dto;

        }
        public async Task<SoloGameDtos?> GetSoloAsync(CancellationToken ct = default)
        {
            var SessionId = _sessionCache.SessionId;
            var dto = await _http.GetFromJsonAsync<SoloGameDtos>($"{SCREEN_ROUTE}/sologame?sessionId={SessionId}");
            return dto;
        }
    }
}
