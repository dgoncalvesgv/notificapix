namespace NotificaPix.Core.Contracts.Requests;

public record AlertTestRequest(decimal Amount, string PayerName, string PayerKey, string Description);
