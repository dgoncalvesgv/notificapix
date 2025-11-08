namespace NotificaPix.Core.Contracts.Responses;

public record UsageResponse(int UsageCount, int Quota, DateTime UsageMonthStartsAt);
