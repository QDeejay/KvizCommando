using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;

    public LoggingHandler(ILogger<LoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("➡️ Request: {Method} {Uri}", request.Method, request.RequestUri);

        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation("⬅️ Response: {StatusCode}", response.StatusCode);

        return response;
    }
}
