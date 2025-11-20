using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Api.Infrastructure;

internal static class PixTransactionUpserter
{
    internal static async Task<TransactionUpsertResult> UpsertAsync(
        NotificaPixDbContext context,
        Guid organizationId,
        IReadOnlyCollection<PixTransaction> transactions,
        CancellationToken cancellationToken)
    {
        if (transactions.Count == 0)
        {
            return new TransactionUpsertResult(0, 0);
        }

        var endToEndIds = transactions
            .Select(t => t.EndToEndId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var txIds = transactions
            .Select(t => t.TxId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existing = await context.PixTransactions
            .Where(t => t.OrganizationId == organizationId &&
                       (endToEndIds.Contains(t.EndToEndId) || txIds.Contains(t.TxId)))
            .ToListAsync(cancellationToken);

        var existingByEndToEnd = existing
            .Where(t => !string.IsNullOrWhiteSpace(t.EndToEndId))
            .GroupBy(t => t.EndToEndId!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var existingByTxId = existing
            .Where(t => !string.IsNullOrWhiteSpace(t.TxId))
            .GroupBy(t => t.TxId!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var toInsert = new List<PixTransaction>();
        var updated = 0;

        foreach (var incoming in transactions)
        {
            if (TryGetExisting(incoming, existingByEndToEnd, existingByTxId, out var current) && current is not null)
            {
                ApplyUpdates(current, incoming);
                updated++;
                continue;
            }

            toInsert.Add(incoming);
        }

        if (toInsert.Count > 0)
        {
            await context.PixTransactions.AddRangeAsync(toInsert, cancellationToken);
        }

        return new TransactionUpsertResult(toInsert.Count, updated);
    }

    private static bool TryGetExisting(
        PixTransaction incoming,
        IDictionary<string, PixTransaction> byEndToEnd,
        IDictionary<string, PixTransaction> byTx,
        out PixTransaction? existing)
    {
        existing = null;
        if (!string.IsNullOrWhiteSpace(incoming.EndToEndId) &&
            byEndToEnd.TryGetValue(incoming.EndToEndId, out existing))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(incoming.TxId) &&
            byTx.TryGetValue(incoming.TxId, out existing))
        {
            return true;
        }

        return false;
    }

    private static void ApplyUpdates(PixTransaction target, PixTransaction source)
    {
        target.TxId = source.TxId;
        target.EndToEndId = source.EndToEndId;
        target.Amount = source.Amount;
        target.OccurredAt = source.OccurredAt;
        target.PayerName = source.PayerName;
        target.PayerKey = source.PayerKey;
        target.Description = source.Description;
        target.RawJson = source.RawJson;
    }

    internal readonly record struct TransactionUpsertResult(int Inserted, int Updated)
    {
        public int Total => Inserted + Updated;
    }
}
