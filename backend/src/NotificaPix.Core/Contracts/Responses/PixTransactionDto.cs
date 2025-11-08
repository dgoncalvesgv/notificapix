namespace NotificaPix.Core.Contracts.Responses;

public record PixTransactionDto(
    Guid Id,
    string TxId,
    string EndToEndId,
    decimal Amount,
    DateTime OccurredAt,
    string PayerName,
    string PayerKey,
    string Description);
