using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Contracts.SoloGame;
using System.Net.Http.Json;

namespace KvizCommando.Client.Features.Solo.Services;

public sealed class SoloGameClientService : ISoloGameClientService
{
    private const string ROUTE = "/api/sologame";

    private readonly HttpClient _http;
    private readonly SessionService _session;
    private readonly ILogger<SoloGameClientService> _logger;

    public SoloGameClientService(
        HttpClient http,
        SessionService session,
        ILogger<SoloGameClientService> logger)
    {
        _http = http;
        _session = session;
        _logger = logger;
    }

    public async Task<StartSoloGameResponse?> StartAsync(
        StartSoloGameRequest request,
        CancellationToken ct = default)
    {
        request.SessionId = GetSessionId();

        try
        {
            using var response = await _http.PostAsJsonAsync($"{ROUTE}/start", request, ct);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<StartSoloGameResponse>(cancellationToken: ct)
                : null;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start solo game.");
            return null;
        }
    }

    public async Task<FinishSoloGameResponse?> FinishAsync(
        Guid gameId,
        FinishSoloGameRequest request,
        CancellationToken ct = default)
    {
        request.SessionId = GetSessionId();

        try
        {
            using var response = await _http.PostAsJsonAsync(
                $"{ROUTE}/{gameId}/finish",
                request,
                ct);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<FinishSoloGameResponse>(cancellationToken: ct)
                : null;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to finish solo game. gameId={GameId}", gameId);
            return null;
        }
    }

    public async Task<bool> AbandonAsync(
        Guid gameId,
        CancellationToken ct = default)
    {
        var request = new AbandonSoloGameRequest
        {
            SessionId = GetSessionId()
        };

        try
        {
            using var response = await _http.PostAsJsonAsync(
                $"{ROUTE}/{gameId}/abandon",
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
            _logger.LogError(ex, "Failed to abandon solo game. gameId={GameId}", gameId);
            return false;
        }
    }

    private string GetSessionId() => _session.SessionId ?? "NoId";
}
