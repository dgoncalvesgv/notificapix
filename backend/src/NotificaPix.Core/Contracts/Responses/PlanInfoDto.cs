using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Contracts.Responses;

public record PlanInfoDto(
    PlanType Plan,
    string DisplayName,
    string PriceText,
    int MonthlyTransactions,
    int TeamMembersLimit,
    int BankAccountsLimit);
