using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Models.Dtos;
using System.Net.Http.Json;

namespace KvizCommando.Client.Services.Dto
{
    public sealed class CacheApiService : ICacheApiService
    {
        private readonly HttpClient _http;
        private readonly SessionService _sessionCache;
        private const string SCREEN_ROUTE = "/api/screen";
        private const string SCREEN_ROUTE_QUESTION = "/api/question/qscreen";
        private const string SCREEN_ROUTE_TEAM = "/api/team/tscreen";
        private const string SCREEN_ROUTE_SOLO = "/api/sologame/screen";
        private const string SCREEN_ROUTE_HOME = "/api/home/screen";
        public CacheApiService(HttpClient http, SessionService sessioncache)
        {
            _http = http;
            _sessionCache = sessioncache;
        }
        public async Task<HomeDTOs?> GetHomeScreenAsync(CancellationToken ct = default)
        {
            var SessionId = _sessionCache.SessionId;
            var dto = await _http.GetFromJsonAsync<HomeDTOs>($"{SCREEN_ROUTE}/home?sessionId={SessionId}", ct);
            return dto;

        }
        public async Task<QuestionDtos?> GetQuestionAsync(CancellationToken ct = default)
        {
            var SessionId = _sessionCache.SessionId;
            var dto = await _http.GetFromJsonAsync<QuestionDtos>($"{SCREEN_ROUTE_QUESTION}?sessionId={SessionId}", ct);
            return dto;
        }
        public async Task<TeamDtos?> GetTeamAsync(CancellationToken ct = default)
        {
            var SessionId = _sessionCache.SessionId;
            var dto = await _http.GetFromJsonAsync<TeamDtos>($"{SCREEN_ROUTE_TEAM}?sessionId={SessionId}", ct);
            return dto;

        }
        public async Task<SoloGameDtos?> GetSoloAsync(CancellationToken ct = default)
        {
            var SessionId = _sessionCache.SessionId;
            var dto = await _http.GetFromJsonAsync<SoloGameDtos>($"{SCREEN_ROUTE}/sologame?sessionId={SessionId}", ct);
            return dto;
        }
    }
}
