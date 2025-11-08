namespace NotificaPix.Core.Contracts.Responses;

public record ApiKeyCreatedResponse(ApiKeyDto Key, string Secret);
