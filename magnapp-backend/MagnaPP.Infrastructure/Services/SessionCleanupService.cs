using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MagnaPP.Infrastructure.Services;

public class SessionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(1);

    public SessionCleanupService(IServiceProvider serviceProvider, ILogger<SessionCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupAsync();
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during session cleanup");
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }

        _logger.LogInformation("Session cleanup service stopped");
    }

    private async Task PerformCleanupAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        
        await sessionService.CleanupExpiredSessionsAsync();
    }
}