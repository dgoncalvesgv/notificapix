namespace NotificaPix.Core.Contracts.Responses;

public record BankSyncResultDto(int IntegrationsProcessed, int TransactionsImported, string Message);
