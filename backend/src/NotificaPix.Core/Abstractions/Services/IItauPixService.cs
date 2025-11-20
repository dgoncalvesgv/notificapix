using NotificaPix.Core.Contracts.Common;
using NotificaPix.Core.Domain.Entities;

namespace NotificaPix.Core.Abstractions.Services;

public interface IItauPixService
{
    Task<IReadOnlyCollection<PixTransaction>> FetchTransactionsAsync(
        BankApiIntegration integration,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);

    Task<ItauPixTestResult> TestCredentialsAsync(
        BankApiIntegration integration,
        bool useProduction,
        CancellationToken cancellationToken);
}
