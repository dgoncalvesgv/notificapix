namespace NotificaPix.Core.Contracts.Responses;

public record PixStaticQrCodeDto(
    Guid Id,
    decimal Amount,
    string Description,
    string Payload,
    string TxId,
    string ReceiverLabel,
    DateTime CreatedAt);
