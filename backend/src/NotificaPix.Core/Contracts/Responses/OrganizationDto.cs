using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Contracts.Responses;

public record OrganizationDto(
    Guid Id,
    string Name,
    string Slug,
    PlanType Plan,
    int UsageCount,
    int Quota,
    string BillingEmail,
    string PlanDisplayName,
    string PlanPriceText,
    int TeamMembersLimit,
    int BankAccountsLimit);
