using KvizCommando.Client.Helpers;
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
        public ApiService(
            HttpClient http,
            IHomeState home,
            IQuestionState questionstate, 
            ITeamState teamstate)
        {
            _http = http;
            _home = home;
            _question = questionstate;
            _team = teamstate;
        }
        public async Task<(bool Success, string Message)> SaveFactorySlotsAsync(SaveFactoryRequest dto)
        {
            string? msg = string.Empty;
            try
            {

                //var response = await _apiClient.PostRawAsync("/api/questions/factory", dto);
                var response = await _http.PostAsJsonAsync("/api/question/factory", dto);
                msg = await ServerRespManager.GetValueAsync(response, "message");

                if (!response.IsSuccessStatusCode)
                {
                    return (false, msg ?? "");
                }
                return (true, msg ?? "");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return (false, msg ?? "");
            }
            finally
            {
                Console.WriteLine("SaveFinished");
            }
        }
        public async Task<(bool Success, string Message)> ManageSlotAsync(ManageSlotRequest dto)
        {
            string? msg = string.Empty;
            try
            {
                var response = await _http.PostAsJsonAsync($"/api/question/manageslot", dto);
                msg = await ServerRespManager.GetValueAsync(response, "message");

                if (!response.IsSuccessStatusCode)
                {
                    return (false, msg ?? "");
                }
                await _home.RefreshAsync();
                return (true, msg ?? "");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return (false, msg ?? "");
            }
            finally
            { Console.WriteLine("Séot managment finished"); }
        }
        public async Task<(bool Success, string Message)> SendNewQuestionAsync(NewQuestionRequest dto)
        {
            string? msg = string.Empty;
            try
            {
                var response = await _http.PostAsJsonAsync($"/api/question/sendnew", dto);
                msg = await ServerRespManager.GetValueAsync(response, "message");

                if (!response.IsSuccessStatusCode)
                {
                    return (false, msg ?? "");
                }
                return (true, msg ?? "");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return (false, msg ?? "");
            }
            finally
            { Console.WriteLine("Send new question finished"); }
        }
        public async Task<(bool Success, string Message)> ModifyTeamAsync(ModifySkillRequest dto)
        {
            string? msg = string.Empty;
            try
            {
                var response = await _http.PostAsJsonAsync($"/api/team/modify", dto);
                msg = await ServerRespManager.GetValueAsync(response, "message");

                if (!response.IsSuccessStatusCode)
                {
                    return (false, msg ?? "");
                }
                return (true, msg ?? "");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return (false, msg ?? "");
            }
            finally
            { Console.WriteLine("Modify teamskill finished"); }
        }
        public async Task<(bool Success, string Message)> ManageTeamAsync(ManageTeamRequest dto)
        {
            string? msg = string.Empty;
            try
            {
                var response = await _http.PostAsJsonAsync($"/api/team/manage", dto);
                msg = await ServerRespManager.GetValueAsync(response, "message");

                if (!response.IsSuccessStatusCode)
                {
                    return (false, msg ?? "");
                }
                await _home.RefreshAsync();
                await _question.RefreshAsync();
                return (true, msg ?? "");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return (false, msg ?? "");
            }
            finally
            { Console.WriteLine("Manage  finished"); }
        }
    }
}
