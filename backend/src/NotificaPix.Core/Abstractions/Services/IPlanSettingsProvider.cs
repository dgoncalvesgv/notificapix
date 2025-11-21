using NotificaPix.Core.Domain.Enums;
using NotificaPix.Core.Options;

namespace NotificaPix.Core.Abstractions.Services;

public interface IPlanSettingsProvider
{
    PlanSetting Get(PlanType plan);
    IEnumerable<(PlanType Plan, PlanSetting Settings)> GetAll();
}
