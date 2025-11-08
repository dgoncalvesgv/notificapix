using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Core.Domain.Enums;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Infrastructure.Workers;

public class AlertDispatcherWorker(IServiceProvider serviceProvider, ILogger<AlertDispatcherWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<NotificaPixDbContext>();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                var webhookDispatcher = scope.ServiceProvider.GetRequiredService<IWebhookDispatcher>();
                var webhookSigner = scope.ServiceProvider.GetRequiredService<IWebhookSigner>();
                var usageService = scope.ServiceProvider.GetRequiredService<IUsageService>();

                var pendingTransactions = await context.PixTransactions
                    .Where(t => !context.Alerts.Any(a => a.PixTransactionId == t.Id))
                    .OrderBy(t => t.OccurredAt)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var transaction in pendingTransactions)
                {
                    var organization = await context.Organizations.Include(o => o.NotificationSettings)
                        .FirstAsync(o => o.Id == transaction.OrganizationId, stoppingToken);

                    var payload = JsonSerializer.Serialize(new
                    {
                        transaction.TxId,
                        transaction.Amount,
                        transaction.PayerName,
                        transaction.PayerKey,
                        occurredAt = transaction.OccurredAt
                    });

                    if (!await usageService.TryConsumeAsync(organization, 1, stoppingToken))
                    {
                        logger.LogWarning("Organization {Org} reached usage quota, skipping alert.", organization.Id);
                        continue;
                    }

                    var alerts = new List<Alert>();
                    var settings = organization.NotificationSettings;
                    if (settings?.Enabled != true)
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(settings.EmailsCsv))
                    {
                        foreach (var email in settings.EmailsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        {
                            await emailSender.SendAsync(email, $"Novo PIX recebido {transaction.Amount:C}", payload, stoppingToken);
                        }
                        alerts.Add(CreateAlert(transaction, AlertChannel.Email, payload));
                    }

                    if (!string.IsNullOrWhiteSpace(settings.WebhookUrl) && !string.IsNullOrWhiteSpace(settings.WebhookSecret))
                    {
                        var signature = webhookSigner.Sign(settings.WebhookSecret, payload);
                        try
                        {
                            await webhookDispatcher.DispatchAsync(settings.WebhookUrl, payload, signature, stoppingToken);
                            alerts.Add(CreateAlert(transaction, AlertChannel.Webhook, payload, AlertStatus.Sent));
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Webhook alert failed for org {Org}", organization.Id);
                            alerts.Add(CreateAlert(transaction, AlertChannel.Webhook, payload, AlertStatus.Failed, ex.Message));
                        }
                    }

                    context.Alerts.AddRange(alerts);
                    await context.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Alert dispatcher failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }
    }

    private static Alert CreateAlert(PixTransaction transaction, AlertChannel channel, string payload, AlertStatus status = AlertStatus.Sent, string? error = null) =>
        new()
        {
            OrganizationId = transaction.OrganizationId,
            PixTransactionId = transaction.Id,
            Channel = channel,
            Status = status,
            Attempts = 1,
            LastAttemptAt = DateTime.UtcNow,
            PayloadJson = payload,
            ErrorMessage = error
        };
}
