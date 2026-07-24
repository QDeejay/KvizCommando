using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Contracts.Question;
using System.Net.Http.Json;

namespace KvizCommando.Client.Features.Question.Services;

public sealed class QuestionClientService : IQuestionClientService
{
    private const string SCREEN_ROUTE_QUESTION = "/api/question";

    private readonly HttpClient _http;
    private readonly SessionService _session;
    private readonly ILogger<QuestionClientService> _logger;

    public QuestionClientService(
        HttpClient http,
        SessionService session,
        ILogger<QuestionClientService> logger)
    {
        _http = http;
        _session = session;
        _logger = logger;
    }

    public Task<bool> SaveFactorySlotsAsync(
        SaveFactoryRequest request,
        CancellationToken ct = default)
    {
        request.SessionId = GetSessionId();
        return PostAsync("factory", request, ct);
    }

    public Task<bool> ManageSlotAsync(
        ManageSlotRequest request,
        CancellationToken ct = default)
    {
        request.SessionId = GetSessionId();
        return PostAsync("manageslot", request, ct);
    }

    public Task<bool> SendNewQuestionAsync(
        NewQuestionRequest request,
        CancellationToken ct = default)
    {
        request.SessionId = GetSessionId();
        return PostAsync("sendnew", request, ct);
    }

    private async Task<bool> PostAsync<TRequest>(
        string action,
        TRequest request,
        CancellationToken ct)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync(
                $"{SCREEN_ROUTE_QUESTION}/{action}",
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
                "Question request failed. Action={Action}",
                action);
            return false;
        }
    }

    private string GetSessionId() => _session.SessionId ?? "NoId";
}
