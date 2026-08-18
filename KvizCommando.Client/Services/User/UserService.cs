using Blazored.SessionStorage;
using KvizCommando.Client.Models.StoreModels;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Shared.Contracts.Auth;
using KvizCommando.Shared.Contracts.CheckIn;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;


namespace KvizCommando.Client.Services.User
{
    public sealed class UserService : IUserService
    {
        private readonly IHomeState _home;
        private const string CheckInRoute = "api/checkin";
        private const string CheckInCacheKey = "checkin:status";

        private readonly HttpClient _http;
        private readonly ISessionStorageService _session;
        private readonly NavigationManager _nav;
        private readonly AudioService _audio;
        private readonly SessionService _sessionCache;
        public UserService(

            IHomeState home,
            HttpClient http,
            ISessionStorageService session,
            NavigationManager navigationManager,
            AudioService audio,
            SessionService sessionCache
            )
        {
            _home = home ?? throw new ArgumentNullException(nameof(home));
            _http = http;
            _session = session;
            _nav = navigationManager;
            _audio = audio;
            _sessionCache = sessionCache;
        }

        public async Task<(bool Success, string Errors)> LoginAsync(LoginRequestForm formData)
        {
            // Query param a RememberMe logikának megfelelően
            var url = formData.rememberMe
                ? "login?useCookies=true"
                : "login?useCookies=true&useSessionCookies=true";
            var dto = new LoginRequestDto()
            {
                Email = formData.Email,
                Password = formData.Password,
                twoFactorCode = formData.twoFactorCode,
                twoFactorRecoveryCode = formData.twoFactorRecoveryCode
            };

            var response = await _http.PostAsJsonAsync(url, dto);

            if (response.IsSuccessStatusCode)
            {
                // Sikeres login: HttpOnly cookie-t a böngésző kezeli, nem kell JSON-t olvasni
                return (true, "");
            }

            return (false, "identityerrors.LoginFailed");
        }
        public async Task LogoutAsync(bool soft)
        {
            var SessionId = _sessionCache.SessionId;
            HttpResponseMessage response;

            try
            {
                response = await _http.PostAsJsonAsync("api/logout", SessionId);
            }
            catch (HttpRequestException) when (!soft)
            {
                await CompleteClientLogoutAsync("error");
                return;
            }
            catch (TaskCanceledException) when (!soft)
            {
                await CompleteClientLogoutAsync("error");
                return;
            }

            if (soft)
            {
                if (!response.IsSuccessStatusCode)
                    return;

                await _audio.StopAllAsync();
                _nav.NavigateTo("/");
                return;
            }

            var reason = response.StatusCode switch
            {
                HttpStatusCode.Conflict => "session",
                HttpStatusCode.Unauthorized => "expired",
                _ when response.IsSuccessStatusCode => "success",
                _ => "error"
            };

            await CompleteClientLogoutAsync(reason);
        }

        private async Task CompleteClientLogoutAsync(string reason)
        {
            await _audio.StopAllAsync();
            await _session.ClearAsync();
            _home.Clear();
            _sessionCache.Clear();
            _nav.NavigateTo($"/?reason={reason}", forceLoad: true);
        }
        public async Task<bool> ConfirmEmailAsync(string userId, string code)
        {

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                return false;
            }

            try
            {
                var response = await _http.GetAsync($"/confirmEmail?userId={userId}&code={code}");
                if (response.IsSuccessStatusCode) { return true; }
                else { return false; }

            }
            catch
            {
                return false;
            }

        }
        public async Task<bool> ForgotPswAsync(ForgotPasswordRequestForm formData)
        {
            // 
            var dto = new ForgotPasswordRequestForm()
            {
                email = formData.email
            };
            var response = await _http.PostAsJsonAsync($"/forgotPassword", dto);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return true;
            }



        }
        public async Task<(bool Success, List<string> Errors)> RecoverPasswordAsync(ResetPasswordForm formData)
        {
            var dto = new ResetPasswordDto()
            {
                email = formData.email,
                resetCode = formData.resetCode,
                newPassword = formData.newPassword
            };

            var response = await _http.PostAsJsonAsync($"/resetpassword", dto);
            if (response.IsSuccessStatusCode)
            {
                return (true, new List<string>());
            }

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return (false, new List<string> { "DefaultError" });

            var problem = JsonSerializer.Deserialize<IdentityProblemDetails>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (problem?.Errors != null && problem.Errors.Count > 0)
            {
                // simán visszaadjuk a szerver által adott kulcsokat
                var ErrorKeyList = problem.Errors.Keys.ToList();
                return (false, ErrorKeyList);
            }

            return (false, new List<string> { "DefaultError" });
        }
        public Task<bool> ProfileDeleteAsync() => Task.FromResult(true);
        public async Task<(bool Success, List<string> Errors)> ProfileRegistAsync(RegisterRequestForm formData)
        {
            var dto = new RegisterRequestDto()
            {
                Email = formData.Email,
                Password = formData.Password
            };


            var response = await _http.PostAsJsonAsync("register", dto);

            if (response.IsSuccessStatusCode)
            {
                return (true, new List<string>());
            }

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return (false, new List<string> { "DefaultError" });

            var problem = JsonSerializer.Deserialize<IdentityProblemDetails>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (problem?.Errors != null && problem.Errors.Count > 0)
            {
                // simán visszaadjuk a szerver által adott kulcsokat
                var ErrorKeyList = problem.Errors.Keys.ToList();
                return (false, ErrorKeyList);
            }

            return (false, new List<string> { "DefaultError" });
        }
        public async Task<(bool CanNavigateHome, List<string> Errors)> CheckInStartAsync(bool needToRoute, CancellationToken ct = default)
        {
            string sessionId = Guid.NewGuid().ToString("N");

            var resp = await _http.GetAsync($"{CheckInRoute}?sessionId={sessionId}", ct);


            if (resp.StatusCode == HttpStatusCode.Unauthorized)
                return (false, new List<string> { "Unauthorized" });

            if (!resp.IsSuccessStatusCode)
                return (false, new List<string> { "DefaultError" });

            var dto = await resp.Content.ReadFromJsonAsync<CheckInGetResponse>(cancellationToken: ct);

            if (dto is null || dto.CurrentTerms is null)
                return (false, new List<string> { "DefaultError" });

            if (dto.NeedsDisplayName || dto.NeedsTermsAcceptance)
            {
                var cacheData = new CheckInSessionCache()
                {
                    needsName = dto.NeedsDisplayName,
                    needsTerms = dto.NeedsTermsAcceptance,
                    termsVersion = dto.CurrentTerms.Version,
                    url = dto.CurrentTerms.Url,
                    PublishedAt = dto.CurrentTerms.PublishedAt
                };
                await _session.SetItemAsync(CheckInCacheKey, cacheData, ct);

                if (needToRoute)
                    _nav.NavigateTo("/checkin");

                return (false, new List<string>());
            }

            await _session.SetItemAsync("SessionId", sessionId, ct);
            _sessionCache.SessionId = sessionId;
            _sessionCache.PendingSessionReplacementWarning = dto.PreviousSessionReplaced;
            await _audio.SetMutedAsync(false);
            return (true, new List<string>());
        }
        /// <summary>
        /// POST /api/checkin → siker: 204 → (true, []); hiba: 4xx + ProblemDetails → (false, ErrorKeys)
        /// </summary>
        public async Task<(bool Success, List<string> Errors, string SugDispName)> CheckInFinishedAsync(CheckInPostRequest request, CancellationToken ct = default)
        {
            var suggestedName = string.Empty;
            var sessionId = Guid.NewGuid().ToString("N");
            request.SessionId = sessionId;
            var errors = new List<string> { "DefaultError" };
            var resp = await _http.PostAsJsonAsync(CheckInRoute, request, cancellationToken: ct);
            var content = await resp.Content.ReadFromJsonAsync<CheckInPostResponse>(ct);

            if (resp.IsSuccessStatusCode && content?.Success == true)
            {
                await _session.SetItemAsync("SessionId", sessionId, ct);
                _sessionCache.SessionId = sessionId;
                _sessionCache.PendingSessionReplacementWarning = content.PreviousSessionReplaced;
                await _audio.SetMutedAsync(false);
                return (true, new List<string>(), "");
            }

            if (content is null)
                return (false, errors, "");

            suggestedName = content?.SuggestedDisplayName ?? string.Empty;

            return (false, content!.Errors ?? errors, suggestedName);
        }
    }
}
