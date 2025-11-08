using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Core.Domain.Enums;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Infrastructure.Services;

public class UsageService(NotificaPixDbContext context, ILogger<UsageService> logger) : IUsageService
{
    public int ResolveQuota(Organization organization) =>
        organization.Plan switch
        {
            PlanType.Pro => 1000,
            PlanType.Business => int.MaxValue,
            _ => 100
        };

    public async Task<bool> TryConsumeAsync(Organization organization, int amount, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        if (organization.UsageMonth.Month != now.Month || organization.UsageMonth.Year != now.Year)
        {
            organization.UsageMonth = new DateTime(now.Year, now.Month, 1);
            organization.UsageCount = 0;
        }

        var quota = ResolveQuota(organization);
        if (organization.UsageCount + amount > quota)
        {
            logger.LogWarning("Organization {Org} exceeded monthly quota {Quota}", organization.Id, quota);
            return false;
        }

        organization.UsageCount += amount;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
