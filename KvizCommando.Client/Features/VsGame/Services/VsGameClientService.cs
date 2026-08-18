using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Contracts.VsGame;
using System.Net.Http.Json;

namespace KvizCommando.Client.Features.VsGame.Services;

public sealed class VsGameClientService : IVsGameClientService
{
    private const string VS_GAME_ROUTE = "/api/vsgame";

    private readonly HttpClient _http;
    private readonly SessionService _session;
    private readonly ILogger<VsGameClientService> _logger;

    public VsGameClientService(
        HttpClient http,
        SessionService session,
        ILogger<VsGameClientService> logger)
    {
        _http = http;
        _session = session;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> SaveBattleTeamAsync(
        SaveBattleTeamRequest request,
        CancellationToken ct = default)
    {
        request.SessionId = _session.SessionId ?? "NoId";

        try
        {
            using var response = await _http.PostAsJsonAsync(
                $"{VS_GAME_ROUTE}/battle-team",
                request,
                ct);

            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException)
            when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "VS battle team save request failed.");
            return false;
        }
    }
}
