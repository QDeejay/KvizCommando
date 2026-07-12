using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Contracts.Team;
using System.Net.Http.Json;

namespace KvizCommando.Client.Services.Dto
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _http;
        private readonly IHomeState _home;
        private readonly IQuestionState _question;
        private readonly ITeamState _team;
        private readonly SessionService _sessionCache;
        public ApiService(
            HttpClient http,
            IHomeState home,
            IQuestionState questionstate,
            ITeamState teamstate,
            SessionService sessioncache)
        {
            _http = http;
            _home = home;
            _question = questionstate;
            _team = teamstate;
            _sessionCache = sessioncache;
        }
        public async Task<bool> SaveFactorySlotsAsync(SaveFactoryRequest dto)
        {

            dto.SessionId = _sessionCache.SessionId ?? "NoId";
            try
            {
                var response = await _http.PostAsJsonAsync("/api/question/factory", dto);

                if (!response.IsSuccessStatusCode)
                    return false;

                _question.Invalidate();
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
                var response = await _http.PostAsJsonAsync($"/api/question/manageslot", dto);

                if (!response.IsSuccessStatusCode)
                    return false;

                _question.Invalidate();
                _home.Invalidate();

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
                var response = await _http.PostAsJsonAsync($"/api/question/sendnew", dto);

                if (!response.IsSuccessStatusCode)
                    return false;

                _question.Invalidate();

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
                var response = await _http.PostAsJsonAsync($"/api/team/modify", dto);


                if (!response.IsSuccessStatusCode)
                    return false;

                _team.Invalidate();
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
                var response = await _http.PostAsJsonAsync($"/api/team/manage", dto);

                if (!response.IsSuccessStatusCode)
                    return false;

                _home.Invalidate();
                _team.Invalidate();
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
