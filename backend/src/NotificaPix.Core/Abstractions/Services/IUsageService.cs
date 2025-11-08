using NotificaPix.Core.Domain.Entities;

namespace NotificaPix.Core.Abstractions.Services;

public interface IUsageService
{
    int ResolveQuota(Organization organization);
    Task<bool> TryConsumeAsync(Organization organization, int amount, CancellationToken cancellationToken);
}
