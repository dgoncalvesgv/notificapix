namespace NotificaPix.Core.Contracts.Responses;

public record OverviewMetricsResponse(
    decimal TodayTotal,
    decimal Last7DaysTotal,
    decimal Last30DaysTotal,
    IReadOnlyCollection<PixTransactionDto> RecentTransactions,
    int AlertsSentToday,
    int ActiveBankConnections);
