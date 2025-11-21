namespace NotificaPix.Core.Contracts.Responses;

public record PixStaticQrCodeDto(
    Guid Id,
    decimal Amount,
    string Payload,
    string TxId,
    string ReceiverLabel,
    DateTime CreatedAt);
