namespace NotificaPix.Core.Contracts.Requests;

public record BankConnectCallbackRequest(string ConsentId, string Code, string? State);
