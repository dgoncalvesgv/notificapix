using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Core.Domain.Enums;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Infrastructure.Workers;

public class PixPollingWorker(IServiceProvider serviceProvider, ILogger<PixPollingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<NotificaPixDbContext>();
                var provider = scope.ServiceProvider.GetRequiredService<IOpenFinanceProvider>();

                var connections = await context.BankConnections
                    .Where(b => b.Status == BankConnectionStatus.Active)
                    .ToListAsync(stoppingToken);

                foreach (var connection in connections)
                {
                    await provider.FetchTransactionsAsync(connection, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "PIX polling worker failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
