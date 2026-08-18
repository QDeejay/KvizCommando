using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Models.Dtos;
using System.Net;
using System.Net.Http.Json;

namespace KvizCommando.Client.Services.Dto
{
    public sealed class CacheApiService : ICacheApiService
    {
        private readonly HttpClient _http;
        private readonly SessionService _sessionCache;
        private const string SCREEN_ROUTE = "/api/screen";
        private const string SCREEN_ROUTE_QUESTION = "/api/question/screen";
        private const string SCREEN_ROUTE_TEAM = "/api/team/screen";
        private const string SCREEN_ROUTE_HOME = "/api/home/screen";
        public CacheApiService(HttpClient http, SessionService sessioncache)
        {
            _http = http;
            _sessionCache = sessioncache;
        }

        public Task<HomeDTOs?> GetHomeScreenAsync(CancellationToken ct = default)
        {
            var sessionId = _sessionCache.SessionId;

            return GetAsync<HomeDTOs>($"{SCREEN_ROUTE}/home?sessionId={sessionId}", ct);
        }

        public Task<QuestionDtos?> GetQuestionAsync(
           CancellationToken ct = default)
        {
            var sessionId = _sessionCache.SessionId;

            return GetAsync<QuestionDtos>(
                $"{SCREEN_ROUTE_QUESTION}?sessionId={sessionId}",
                ct);
        }

        public Task<TeamDtos?> GetTeamAsync(
            CancellationToken ct = default)
        {
            var sessionId = _sessionCache.SessionId;

            return GetAsync<TeamDtos>(
                $"{SCREEN_ROUTE_TEAM}?sessionId={sessionId}",
                ct);
        }

        public Task<SoloGameDtos?> GetSoloAsync(
            CancellationToken ct = default)
        {
            var sessionId = _sessionCache.SessionId;

            return GetAsync<SoloGameDtos>(
                $"{SCREEN_ROUTE}/sologame?sessionId={sessionId}",
                ct);
        }

        public Task<VsGameDtos?> GetVsGameAsync(
            CancellationToken ct = default)
        {
            var sessionId = _sessionCache.SessionId;

            return GetAsync<VsGameDtos>(
                $"{SCREEN_ROUTE}/vsgame?sessionId={sessionId}",
                ct);
        }



        private async Task<T?> GetAsync<T>(string route, CancellationToken ct)
        {
            var response = await _http.GetAsync(route, ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return default;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>(
                cancellationToken: ct);
        }
    }

}
/*
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
        public async Task<VsGameDtos?> GetVsGameAsync(CancellationToken ct = default)
        {
            var sessionId = _sessionCache.SessionId;
            return await _http.GetFromJsonAsync<VsGameDtos>($"{SCREEN_ROUTE}/vsgame?sessionId={sessionId}", ct);
        }

 */