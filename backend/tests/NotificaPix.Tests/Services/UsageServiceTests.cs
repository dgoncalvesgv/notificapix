using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Core.Domain.Enums;
using NotificaPix.Infrastructure.Persistence;
using NotificaPix.Infrastructure.Services;

namespace NotificaPix.Tests.Services;

public class UsageServiceTests
{
    [Fact]
    public async Task TryConsumeAsync_ShouldResetCounterOnNewMonth()
    {
        var options = new DbContextOptionsBuilder<NotificaPixDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new NotificaPixDbContext(options);
        var service = new UsageService(context, NullLogger<UsageService>.Instance);

        var organization = new Organization
        {
            Name = "Test",
            Slug = "test",
            Plan = PlanType.Starter,
            UsageCount = 99,
            UsageMonth = DateTime.UtcNow.AddMonths(-1)
        };

        context.Organizations.Add(organization);
        await context.SaveChangesAsync();

        var consumed = await service.TryConsumeAsync(organization, 1, default);

        Assert.True(consumed);
        Assert.Equal(1, organization.UsageCount);
    }
}
