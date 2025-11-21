namespace NotificaPix.Core.Contracts.Responses;

public record PixReceiversResponse(IEnumerable<PixReceiverDto> Options, PixReceiverDto? Selected);
