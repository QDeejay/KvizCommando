using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Contracts.Team;
using System.Net.Http.Json;

namespace KvizCommando.Client.Services.Dto
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _http;
        private readonly SessionService _sessionCache;

        private const string SCREEN_ROUTE_QUESTION = "/api/question";
        private const string SCREEN_ROUTE_TEAM = "/api/team";
        private const string SCREEN_ROUTE_SOLO = "/api/sologame";
        public ApiService(HttpClient http, SessionService sessioncache)
        {
            _http = http;
            _sessionCache = sessioncache;
        }
        public async Task<bool> SaveFactorySlotsAsync(SaveFactoryRequest dto)
        {

            dto.SessionId = _sessionCache.SessionId ?? "NoId";
            try
            {
                var response = await _http.PostAsJsonAsync($"{SCREEN_ROUTE_QUESTION}/factory", dto);

                if (!response.IsSuccessStatusCode)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
            finally
            {
                Console.WriteLine("SaveFinished");
            }
        }
        public async Task<bool> ManageSlotAsync(ManageSlotRequest dto)
        {

            dto.SessionId = _sessionCache.SessionId ?? "NoId";
            try
            {
                var response = await _http.PostAsJsonAsync($"{SCREEN_ROUTE_QUESTION}/manageslot", dto);

                if (!response.IsSuccessStatusCode)
                    return false;


                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
            finally
            {
                Console.WriteLine("Slot managment finished");
            }
        }
        public async Task<bool> SendNewQuestionAsync(NewQuestionRequest dto)
        {

            dto.SessionId = _sessionCache.SessionId ?? "NoId";
            try
            {
                var response = await _http.PostAsJsonAsync($"{SCREEN_ROUTE_QUESTION}/sendnew", dto);

                if (!response.IsSuccessStatusCode)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
            finally
            {
                Console.WriteLine("Send new question finished");
            }
        }
        public async Task<bool> ModifyTeamAsync(ModifySkillRequest dto)
        {
            dto.SessionId = _sessionCache.SessionId ?? "NoId";
            try
            {
                var response = await _http.PostAsJsonAsync($"{SCREEN_ROUTE_TEAM}/modify", dto);


                if (!response.IsSuccessStatusCode)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
            finally
            {
                Console.WriteLine("Modify teamskill finished");
            }
        }
        public async Task<bool> ManageTeamAsync(ManageTeamRequest dto)
        {
            dto.SessionId = _sessionCache.SessionId ?? "NoId";
            try
            {
                var response = await _http.PostAsJsonAsync($"{SCREEN_ROUTE_TEAM}/manage", dto);

                if (!response.IsSuccessStatusCode)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
            finally
            {
                Console.WriteLine("Modify teamskill finished");
            }
        }
    }
}