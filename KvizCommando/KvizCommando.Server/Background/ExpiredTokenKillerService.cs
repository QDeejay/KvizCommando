using KvizCommando.Server.Data;
using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Background;

public class ExpiredTokenKillerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredTokenKillerService> _logger;

    public ExpiredTokenKillerService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredTokenKillerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // indulás után azonnal fut
        await CleanupAsync(stoppingToken);
       
        while (!stoppingToken.IsCancellationRequested)
        {
            // kiszámoljuk mennyi idő van a következő éjfélig
            var now = DateTime.UtcNow;
            var midnight = now.Date.AddDays(1); // következő nap 00:00 UTC
            var delay = midnight - now;

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await CleanupAsync(stoppingToken);
            }
        }
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cutoff = DateTime.UtcNow.AddDays(-30); // 30 napnál régebben lejárt tokenek
        await Task.Delay(10);
       
    }
}
