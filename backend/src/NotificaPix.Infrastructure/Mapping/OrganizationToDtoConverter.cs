using AutoMapper;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Core.Domain.Entities;

namespace NotificaPix.Infrastructure.Mapping;

public class OrganizationToDtoConverter : ITypeConverter<Organization, OrganizationDto>
{
    private readonly IPlanSettingsProvider _planSettingsProvider;

    public OrganizationToDtoConverter(IPlanSettingsProvider planSettingsProvider)
    {
        _planSettingsProvider = planSettingsProvider;
    }

    public OrganizationDto Convert(Organization source, OrganizationDto destination, ResolutionContext context)
    {
        var plan = _planSettingsProvider.Get(source.Plan);

        return new OrganizationDto(
            source.Id,
            source.Name,
            source.Slug,
            source.Plan,
            source.UsageCount,
            plan.MonthlyTransactions,
            source.BillingEmail,
            plan.DisplayName,
            plan.PriceText,
            plan.TeamMembersLimit,
            plan.BankAccountsLimit);
    }
}
