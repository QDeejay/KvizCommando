using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Contracts.Team;
using System.Net.Http.Json;

namespace KvizCommando.Client.Features.Team.Services;

public sealed class TeamClientService : ITeamClientService
{
    private const string SCREEN_ROUTE_TEAM = "/api/team";

    private readonly HttpClient _http;
    private readonly SessionService _session;
    private readonly ILogger<TeamClientService> _logger;

    public TeamClientService(
        HttpClient http,
        SessionService session,
        ILogger<TeamClientService> logger)
    {
        _http = http;
        _session = session;
        _logger = logger;
    }

    public Task<bool> ModifySkillsAsync(
        ModifySkillRequest request,
        CancellationToken ct = default)
    {
        request.SessionId = GetSessionId();
        return PostAsync("modify", request, ct);
    }

    public Task<bool> ManageTeamAsync(
        ManageTeamRequest request,
        CancellationToken ct = default)
    {
        request.SessionId = GetSessionId();
        return PostAsync("manage", request, ct);
    }

    private async Task<bool> PostAsync<TRequest>(
        string action,
        TRequest request,
        CancellationToken ct)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync(
                $"{SCREEN_ROUTE_TEAM}/{action}",
                request,
                ct);

            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Team request failed. Action={Action}",
                action);
            return false;
        }
    }

    private string GetSessionId() => _session.SessionId ?? "NoId";
}
