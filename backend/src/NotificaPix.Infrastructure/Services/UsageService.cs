using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Infrastructure.Persistence;

namespace NotificaPix.Infrastructure.Services;

public class UsageService : IUsageService
{
    private readonly NotificaPixDbContext _context;
    private readonly ILogger<UsageService> _logger;
    private readonly IPlanSettingsProvider _planSettingsProvider;

    public UsageService(NotificaPixDbContext context, ILogger<UsageService> logger, IPlanSettingsProvider planSettingsProvider)
    {
        _context = context;
        _logger = logger;
        _planSettingsProvider = planSettingsProvider;
    }

    public int ResolveQuota(Organization organization) =>
        _planSettingsProvider.Get(organization.Plan).MonthlyTransactions;

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
            _logger.LogWarning("Organization {Org} exceeded monthly quota {Quota}", organization.Id, quota);
            return false;
        }

        organization.UsageCount += amount;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
