using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Core.Domain.Enums;
using NotificaPix.Core.Options;

namespace NotificaPix.Infrastructure.Services;

public class PlanSettingsProvider : IPlanSettingsProvider
{
    private readonly ConcurrentDictionary<PlanType, PlanSetting> _cache = new();
    private readonly PlanSettingsOptions _options;

    public PlanSettingsProvider(IOptions<PlanSettingsOptions> options)
    {
        _options = options.Value ?? new PlanSettingsOptions();
    }

    public PlanSetting Get(PlanType plan) =>
        _cache.GetOrAdd(plan, p => _options.GetSetting(p));

    public IEnumerable<(PlanType Plan, PlanSetting Settings)> GetAll()
    {
        yield return (PlanType.Starter, Get(PlanType.Starter));
        yield return (PlanType.Pro, Get(PlanType.Pro));
        yield return (PlanType.Business, Get(PlanType.Business));
    }
}
