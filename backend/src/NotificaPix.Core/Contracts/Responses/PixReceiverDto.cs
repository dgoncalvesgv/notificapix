namespace NotificaPix.Core.Contracts.Responses;

public record PixReceiverDto(
    Guid Id,
    string Label,
    string KeyType,
    string KeyValue,
    bool IsDefault);
