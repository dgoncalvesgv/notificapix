using NotificaPix.Core.Domain.Entities;
using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Abstractions.Services;

public interface IOpenFinanceProvider
{
    BankProvider Provider { get; }
    Task<string> CreateConsentUrlAsync(Guid organizationId, CancellationToken cancellationToken);
    Task<BankConnection> CompleteConnectionAsync(Guid organizationId, string consentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PixTransaction>> FetchTransactionsAsync(BankConnection connection, CancellationToken cancellationToken);
}
