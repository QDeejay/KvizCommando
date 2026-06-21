using KvizCommando.Client.Features.Sologame;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Contracts.CheckIn;
using KvizCommando.Shared.Models.Dtos;
using System.Net.Http.Json;

namespace KvizCommando.Client.Services.Dto
{
    public sealed class ScreenApiService : IScreenApiService
    {
        private readonly HttpClient _http;
        private readonly SessionService _sessionCache;
        private const string ScreenRoute = "api/screen";
        public ScreenApiService(HttpClient http, SessionService sessioncache)
        {
            _http = http;
            _sessionCache = sessioncache;
        }
        public async Task<HomeDTOs?> GetHomeScreenAsync(CancellationToken ct = default)
        {
            var SessionId = _sessionCache.SessionId;
            var dto = await _http.GetFromJsonAsync<HomeDTOs>($"{ScreenRoute}/home?sessionId={SessionId}");
            //var resp = await _http.GetAsync($"{ScreenRoute}/home?sessionId={SessionId}");
            //var dto = await resp.Content.ReadFromJsonAsync<HomeDTOs>(cancellationToken: ct);
            return dto;

        }
        public async Task<QuestionDtos?> GetQuestionAsync(CancellationToken ct = default)
        {
            var SessionId = _sessionCache.SessionId;
            var dto = await _http.GetFromJsonAsync<QuestionDtos>($"{ScreenRoute}/question?sessionId={SessionId}");
            //var dto = await  response.Content.ReadFromJsonAsync<QuestionDtos>();
            return dto;
        }
        public async Task<TeamDtos?> GetTeamAsync(CancellationToken ct = default)
        {
            var SessionId = _sessionCache.SessionId;
            var dto = await _http.GetFromJsonAsync<TeamDtos>($"{ScreenRoute}/team?sessionId={SessionId}");
            return dto;

        }
        public async Task<SoloGameDtos?> GetSoloAsync(CancellationToken ct = default)
        {
            var SessionId = _sessionCache.SessionId;
            var dto = await _http.GetFromJsonAsync<SoloGameDtos>($"{ScreenRoute}/sologame?sessionId={SessionId}");
            return dto;
        }
        


    }
}
