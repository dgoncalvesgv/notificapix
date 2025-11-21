namespace NotificaPix.Core.Contracts.Requests;

public record CreatePixStaticQrRequest(decimal Amount, Guid? PixKeyId, string? Description);
