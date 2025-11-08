using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Core.Contracts.Requests;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Core.Domain.Enums;
using NotificaPix.Core.Exceptions;
using NotificaPix.Infrastructure.Persistence;
using System.Text.Json;

namespace NotificaPix.Infrastructure.Services;

public class AlertService(
    NotificaPixDbContext context,
    IEmailSender emailSender,
    IWebhookDispatcher webhookDispatcher,
    IWebhookSigner webhookSigner,
    ILogger<AlertService> logger) : IAlertService
{
    public async Task<AlertDto> DispatchTestAlertAsync(Guid organizationId, AlertTestRequest request, CancellationToken cancellationToken)
    {
        var settings = await context.NotificationSettings.FirstOrDefaultAsync(n => n.OrganizationId == organizationId, cancellationToken)
            ?? throw new NotFoundException("Notification settings not configured");

        var payload = JsonSerializer.Serialize(new
        {
            amount = request.Amount,
            payerName = request.PayerName,
            payerKey = request.PayerKey,
            description = request.Description,
            sentAt = DateTime.UtcNow
        });

        var alert = new Alert
        {
            OrganizationId = organizationId,
            Channel = AlertChannel.Email,
            Status = AlertStatus.Sent,
            Attempts = 1,
            PayloadJson = payload,
            LastAttemptAt = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(settings.EmailsCsv))
        {
            foreach (var email in settings.EmailsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                await emailSender.SendAsync(email, "NotificaPix - teste de alerta", payload, cancellationToken);
            }
        }

        if (!string.IsNullOrWhiteSpace(settings.WebhookUrl) && !string.IsNullOrWhiteSpace(settings.WebhookSecret))
        {
            var signature = webhookSigner.Sign(settings.WebhookSecret, payload);
            await webhookDispatcher.DispatchAsync(settings.WebhookUrl, payload, signature, cancellationToken);
        }

        context.Alerts.Add(alert);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Test alert created for org {Org}", organizationId);

        return new AlertDto(alert.Id, alert.Channel, alert.Status, alert.LastAttemptAt, alert.Attempts, alert.PayloadJson);
    }
}
