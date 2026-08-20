using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Contracts.Profile;
using System.Net.Http.Json;

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
}
