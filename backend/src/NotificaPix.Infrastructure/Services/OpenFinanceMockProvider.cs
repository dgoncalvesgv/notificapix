using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Core.Domain.Enums;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Infrastructure.Services;

/// <summary>
/// Mock provider that simulates OAuth connections and generates PIX transactions.
/// </summary>
public class OpenFinanceMockProvider(NotificaPixDbContext context, ILogger<OpenFinanceMockProvider> logger) : IOpenFinanceProvider
{
    private static readonly string[] Names = ["Ana Souza", "Diego Andrade", "Maria Lima", "Pedro Rocha"];
    public BankProvider Provider => BankProvider.Mock;

    public Task<string> CreateConsentUrlAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var url = $"https://mock.openfinance.local/consent?org={organizationId}";
        logger.LogInformation("Generated mock consent URL for org {Org}: {Url}", organizationId, url);
        return Task.FromResult(url);
    }

    public async Task<BankConnection> CompleteConnectionAsync(Guid organizationId, string consentId, CancellationToken cancellationToken)
    {
        var connection = new BankConnection
        {
            OrganizationId = organizationId,
            Provider = Provider,
            ConsentId = consentId,
            Status = BankConnectionStatus.Active,
            ConnectedAt = DateTime.UtcNow,
            MetaJson = """{"provider":"mock"}"""
        };

        context.BankConnections.Add(connection);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Mock bank connection completed for org {Org}", organizationId);
        return connection;
    }

    public async Task<IReadOnlyCollection<PixTransaction>> FetchTransactionsAsync(BankConnection connection, CancellationToken cancellationToken)
    {
        var lastDay = await context.PixTransactions
            .Where(t => t.OrganizationId == connection.OrganizationId)
            .OrderByDescending(t => t.OccurredAt)
            .Select(t => t.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);

        var startDate = lastDay == default ? DateTime.UtcNow.AddHours(-6) : lastDay.AddMinutes(5);
        var transactions = new List<PixTransaction>();
        var random = new Random();

        for (var i = 0; i < random.Next(1, 3); i++)
        {
            var amount = Math.Round((decimal)random.NextDouble() * 2500m, 2);
            var name = Names[random.Next(Names.Length)];
            var tx = new PixTransaction
            {
                OrganizationId = connection.OrganizationId,
                TxId = Guid.NewGuid().ToString("N")[..8],
                EndToEndId = $"E2E{Guid.NewGuid():N}",
                Amount = amount,
                OccurredAt = startDate.AddMinutes(random.Next(10, 120)),
                PayerName = name,
                PayerKey = $"{name[..3].ToLowerInvariant()}@pix.com",
                Description = "PIX recebido - Mock",
                RawJson = """{"source":"mock"}"""
            };
            transactions.Add(tx);
        }

        await context.PixTransactions.AddRangeAsync(transactions, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Generated {Count} mock pix transactions for org {Org}", transactions.Count, connection.OrganizationId);
        return transactions;
    }
}
