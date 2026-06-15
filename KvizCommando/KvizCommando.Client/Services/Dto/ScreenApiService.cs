using KvizCommando.Client.Features.Sologame;
using KvizCommando.Shared.Models.Dtos;
using System.Net.Http.Json;

namespace KvizCommando.Client.Services.Dto
{
    public sealed class ScreenApiService : IScreenApiService
    {
        private readonly HttpClient _http;
        public ScreenApiService(HttpClient http)
        {
            _http = http;
        }
        public async Task<HomeDTOs?> GetHomeScreenAsync()
        {

            var dto = await _http.GetFromJsonAsync<HomeDTOs>("api/screen/home");
            return dto;
        }
        public async Task<QuestionDtos?> GetQuestionAsync()
        {
            var dto = await _http.GetFromJsonAsync<QuestionDtos>("api/screen/question");
            //var dto = await  response.Content.ReadFromJsonAsync<QuestionDtos>();
            return dto;
        }
        public async Task<TeamDtos?> GetTeamAsync()
        {
            var dto = await _http.GetFromJsonAsync<TeamDtos>("api/screen/team");
            return dto;

        }

        public async Task<SoloGameDtos?> GetSoloAsync()
        {
            var dto = await _http.GetFromJsonAsync<SoloGameDtos>("api/screen/sologame");
            return dto;
        }
        


    }
}
