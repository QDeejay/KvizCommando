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
        public CacheApiService(HttpClient http, SessionService sessioncache)
        {
            _http = http;
            _sessionCache = sessioncache;
        }

        /// <inheritdoc />
        public Task<HomeDTOs?> GetHomeScreenAsync(CancellationToken ct = default)
        {
            var sessionId = _sessionCache.SessionId;

            return GetAsync<HomeDTOs>($"{SCREEN_ROUTE}/home?sessionId={sessionId}", ct);
        }

        /// <inheritdoc />
        public Task<QuestionDtos?> GetQuestionAsync(
           CancellationToken ct = default)
        {
            var sessionId = _sessionCache.SessionId;

            return GetAsync<QuestionDtos>($"{SCREEN_ROUTE}/question?sessionId={sessionId}", ct);
        }

        /// <inheritdoc />
        public Task<TeamDtos?> GetTeamAsync(
            CancellationToken ct = default)
        {
            var sessionId = _sessionCache.SessionId;

            return GetAsync<TeamDtos>($"{SCREEN_ROUTE}/team?sessionId={sessionId}", ct);
        }

        /// <inheritdoc />
        public Task<SoloGameDtos?> GetSoloAsync(
            CancellationToken ct = default)
        {
            var sessionId = _sessionCache.SessionId;

            return GetAsync<SoloGameDtos>(
                $"{SCREEN_ROUTE}/sologame?sessionId={sessionId}",
                ct);
        }

        /// <inheritdoc />
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
