using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.User;
using KvizCommando.Shared.Contracts.CheckIn;
using KvizCommando.Shared.Contracts.Profile;
using System.Net.Http.Json;
using System.Text.Json;

namespace KvizCommando.Client.Features.Shared.Profile;

public sealed class ProfileClientService : IProfileClientService
{
    private const string PROFILE_ROUTE = "/api/profile";

    private readonly HttpClient _http;
    private readonly SessionService _session;
    private readonly ILogger<ProfileClientService> _logger;

    public ProfileClientService(
        HttpClient http,
        SessionService session,
        ILogger<ProfileClientService> logger)
    {
        _http = http;
        _session = session;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ProfileLoadResponse> GetAsync(
        CancellationToken ct = default)
    {
        try
        {
            using var response = await _http.GetAsync(
                $"{PROFILE_ROUTE}?sessionId={GetSessionId()}",
                ct);

            if (!response.IsSuccessStatusCode)
                return FailedLoad();

            return await response.Content.ReadFromJsonAsync<ProfileLoadResponse>(
                cancellationToken: ct) ?? FailedLoad();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Profile load request failed.");
            return FailedLoad();
        }
    }

    /// <inheritdoc />
    public Task<CheckTeamNameResponse> CheckTeamNameAsync(
        string teamName,
        CancellationToken ct = default) =>
        PostAsync<CheckTeamNameRequest, CheckTeamNameResponse>(
            "check-teamname",
            new CheckTeamNameRequest
            {
                SessionId = GetSessionId(),
                TeamName = teamName
            },
            new CheckTeamNameResponse
            {
                State = ProfileRequestState.ServerError
            },
            ct);

    /// <inheritdoc />
    public Task<SaveProfileResponse> SaveTeamNameAsync(
        string teamName,
        CancellationToken ct = default) =>
        PostAsync<SaveTeamNameRequest, SaveProfileResponse>(
            "teamname",
            new SaveTeamNameRequest
            {
                SessionId = GetSessionId(),
                TeamName = teamName
            },
            FailedSave(),
            ct);

    /// <inheritdoc />
    public Task<SaveProfileResponse> SaveAvatarAsync(
        string captainAvatar,
        CancellationToken ct = default) =>
        PostAsync<SaveAvatarRequest, SaveProfileResponse>(
            "avatar",
            new SaveAvatarRequest
            {
                SessionId = GetSessionId(),
                CaptainAvatar = captainAvatar
            },
            FailedSave(),
            ct);

    /// <inheritdoc />
    public async Task<ProfileAccountResponse> GetAccountAsync(CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<ProfileAccountResponse>(
                $"{PROFILE_ROUTE}/account", ct) ?? FailedAccount();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Profile account load request failed.");
            return FailedAccount();
        }
    }

    /// <inheritdoc />
    public async Task<ProfileAccountResponse> SaveAccountAsync(
        SaveProfileAccountRequest request,
        CancellationToken ct = default)
    {
        try
        {
            using var response = await _http.PutAsJsonAsync($"{PROFILE_ROUTE}/account", request, ct);
            if (!response.IsSuccessStatusCode)
                return FailedAccount();
            return await response.Content.ReadFromJsonAsync<ProfileAccountResponse>(cancellationToken: ct)
                ?? FailedAccount();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Profile account save request failed.");
            return FailedAccount();
        }
    }

    /// <inheritdoc />
    public Task<ProfileIdentityUpdateResponse> RequestEmailChangeAsync(
        string newEmail,
        CancellationToken ct = default) =>
        UpdateIdentityAsync(new IdentityInfoRequest { NewEmail = newEmail }, ct);

    /// <inheritdoc />
    public Task<ProfileIdentityUpdateResponse> ChangePasswordAsync(
        string currentPassword,
        string newPassword,
        CancellationToken ct = default) =>
        UpdateIdentityAsync(new IdentityInfoRequest
        {
            OldPassword = currentPassword,
            NewPassword = newPassword
        }, ct);

    /// <inheritdoc />
    public async Task<TermsMeta?> GetLegalMetaAsync(
        CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<TermsMeta>(
                $"{PROFILE_ROUTE}/legal",
                ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Profile legal document metadata request failed.");
            return null;
        }
    }

    private async Task<ProfileIdentityUpdateResponse> UpdateIdentityAsync(
        IdentityInfoRequest request,
        CancellationToken ct)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync("/manage/info", request, ct);
            if (response.IsSuccessStatusCode)
                return new ProfileIdentityUpdateResponse { Success = true };

            var json = await response.Content.ReadAsStringAsync(ct);
            var problem = string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<IdentityProblemDetails>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return new ProfileIdentityUpdateResponse
            {
                Errors = problem?.Errors?.SelectMany(x => x.Value).ToList()
                    ?? ["DefaultError"]
            };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Identity profile update request failed.");
            return new ProfileIdentityUpdateResponse { Errors = ["DefaultError"] };
        }
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string action,
        TRequest request,
        TResponse failure,
        CancellationToken ct)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync(
                $"{PROFILE_ROUTE}/{action}",
                request,
                ct);

            if (!response.IsSuccessStatusCode)
                return failure;

            return await response.Content.ReadFromJsonAsync<TResponse>(
                cancellationToken: ct) ?? failure;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Profile request failed. Action={Action}",
                action);
            return failure;
        }
    }

    private string GetSessionId() => _session.SessionId ?? "NoId";

    private static ProfileLoadResponse FailedLoad() => new()
    {
        State = ProfileRequestState.ServerError
    };

    private static SaveProfileResponse FailedSave() => new()
    {
        State = ProfileRequestState.ServerError
    };

    private static ProfileAccountResponse FailedAccount() => new()
    {
        State = ProfileAccountRequestState.ServerError
    };

    private sealed class IdentityInfoRequest
    {
        public string? NewEmail { get; set; }
        public string? NewPassword { get; set; }
        public string? OldPassword { get; set; }
    }
}
