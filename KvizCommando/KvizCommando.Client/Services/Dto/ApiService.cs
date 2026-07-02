using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.StoreModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Enums;
using System.Net.Http.Json;
using static System.Net.Mime.MediaTypeNames;

namespace KvizCommando.Client.Services.Dto
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _http;
        private readonly IHomeState _home;
        private readonly IQuestionState _question;
        private readonly ITeamState _team;
        private readonly SessionService _sessionCache;
        private readonly ToastService _toastService;
        public ApiService(
            HttpClient http,
            IHomeState home,
            IQuestionState questionstate, 
            ITeamState teamstate,
            SessionService sessioncache,
            ToastService toastService)
        {
            _http = http;
            _home = home;
            _question = questionstate;
            _team = teamstate;
            _sessionCache = sessioncache;
            _toastService = toastService;
        }
        public async Task<bool> SaveFactorySlotsAsync(SaveFactoryRequest dto)
        {
            string? msg = string.Empty;
            string? tst;
            var type = new ToastType();
            dto.SessionId = _sessionCache.SessionId ?? "NoId";
            try
            {
                var response = await _http.PostAsJsonAsync("/api/question/factory", dto);
                msg = await ServerRespManager.GetValueAsync(response, "message");
                tst = await ServerRespManager.GetValueAsync(response, "type") ?? "Info";
                type = Enum.Parse<ToastType>(tst);

                if (!response.IsSuccessStatusCode)
                    return false;

                _question.Invalidate();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                msg = ex.Message;
                type = ToastType.Error;
                return false;
            }
            finally
            {
                _toastService.Show(msg ?? "", type);
                Console.WriteLine("SaveFinished");
            }
        }
        public async Task<bool> ManageSlotAsync(ManageSlotRequest dto)
        {
            string? msg = string.Empty; 
            string? tst;
            var type = new ToastType();
            dto.SessionId = _sessionCache.SessionId ?? "NoId";
            try
            {
                var response = await _http.PostAsJsonAsync($"/api/question/manageslot", dto);
                msg = await ServerRespManager.GetValueAsync(response, "message");
                tst = await ServerRespManager.GetValueAsync(response, "type") ?? "Info";
                type = Enum.Parse<ToastType>(tst);

                if (!response.IsSuccessStatusCode)
                    return false;

                _question.Invalidate();
                _home.Invalidate();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                msg = ex.Message;
                type = ToastType.Error;
                return false;
            }
            finally
            {
                _toastService.Show(msg ?? "", type);
                Console.WriteLine("Slot managment finished"); 
            }
        }
        public async Task<bool> SendNewQuestionAsync(NewQuestionRequest dto)
        {
            string? msg = string.Empty;
            string? tst;
            var type = new ToastType();
            dto.SessionId = _sessionCache.SessionId ?? "NoId";
            try
            {
                var response = await _http.PostAsJsonAsync($"/api/question/sendnew", dto);
                msg = await ServerRespManager.GetValueAsync(response, "message");
                tst = await ServerRespManager.GetValueAsync(response, "type") ?? "Info";
                type = Enum.Parse<ToastType>(tst);

                if (!response.IsSuccessStatusCode)
                    return false;
                
                _question.Invalidate();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                msg = ex.Message;
                type = ToastType.Error;
                return false;
            }
            finally
            {
                _toastService.Show(msg ?? "", type);
                Console.WriteLine("Send new question finished"); 
            }
        }
        public async Task<(bool Success, string Message)> ModifyTeamAsync(ModifySkillRequest dto)
        {
            string? msg = string.Empty;
            dto.SessionId = _sessionCache.SessionId ?? "NoId";
            try
            {
                var response = await _http.PostAsJsonAsync($"/api/team/modify", dto);
                msg = await ServerRespManager.GetValueAsync(response, "message");

                if (!response.IsSuccessStatusCode)
                    return (false, msg ?? "");

                _team.Invalidate();
                return (true, msg ?? "");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return (false, ex.Message);
            }
            finally
            { 
                Console.WriteLine("Modify teamskill finished"); 
            }
        }
        public async Task<(bool Success, string Message)> ManageTeamAsync(ManageTeamRequest dto)
        {
            string? msg = string.Empty;
            dto.SessionId = _sessionCache.SessionId ?? "NoId";
            try
            {
                var response = await _http.PostAsJsonAsync($"/api/team/manage", dto);
                msg = await ServerRespManager.GetValueAsync(response, "message");

                if (!response.IsSuccessStatusCode)
                {
                    return (false, msg ?? "");
                }
                _home.Invalidate();
                _team.Invalidate();
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
