using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Core.Domain.Enums;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Infrastructure.Seed;

public class DataSeeder(NotificaPixDbContext context, IPasswordHasher passwordHasher, ILogger<DataSeeder> logger) : IDataSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Organizations.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already seeded");
            return;
        }

        var organization = new Organization
        {
            Name = "Org Demo",
            Slug = "org-demo",
            Plan = PlanType.Starter,
            BillingEmail = "billing@demo.com"
        };

        var user = new User
        {
            Email = "admin@demo.com",
            PasswordHash = passwordHasher.Hash("P@ssword123")
        };

        var membership = new Membership
        {
            Organization = organization,
            User = user,
            Role = MembershipRole.OrgAdmin
        };

        var notificationSettings = new NotificationSettings
        {
            Organization = organization,
            EmailsCsv = "alerts@demo.com",
            WebhookSecret = "secret123",
            WebhookUrl = "http://localhost:7071/mock-webhook",
            Enabled = true
        };

        var connection = new BankConnection
        {
            Organization = organization,
            Provider = BankProvider.Mock,
            ConsentId = "mock-consent",
            Status = BankConnectionStatus.Active,
            ConnectedAt = DateTime.UtcNow
        };

        var random = new Random();
        for (var i = 0; i < 10; i++)
        {
            var pix = new PixTransaction
            {
                Organization = organization,
                TxId = $"TX{i:0000}",
                EndToEndId = $"E2E{i:0000}",
                Amount = random.Next(100, 5000) / 10m,
                OccurredAt = DateTime.UtcNow.AddMinutes(-i * 10),
                PayerName = $"Cliente {i}",
                PayerKey = $"cliente{i}@pix.com",
                Description = "PIX Demo",
                RawJson = """{"source":"seed"}"""
            };

            context.PixTransactions.Add(pix);

            if (i < 5)
            {
                context.Alerts.Add(new Alert
                {
                    Organization = organization,
                    PixTransaction = pix,
                    Channel = i % 2 == 0 ? AlertChannel.Email : AlertChannel.Webhook,
                    Status = AlertStatus.Sent,
                    Attempts = 1,
                    LastAttemptAt = DateTime.UtcNow.AddMinutes(-i * 5),
                    PayloadJson = """{"demo":true}"""
                });
            }
        }

        context.NotificationSettings.Add(notificationSettings);
        context.BankConnections.Add(connection);
        context.Memberships.Add(membership);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seed data created: admin@demo.com / P@ssword123");
    }
}
