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
        // Induláskor is lefut, hogy a leállás alatt lejárt tokenek ne maradjanak a következő ütemezésig.
        await CleanupAsync(stoppingToken);
       
        while (!stoppingToken.IsCancellationRequested)
        {
            // A napi takarítás helyi idő szerint éjfél után indul újra.
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
