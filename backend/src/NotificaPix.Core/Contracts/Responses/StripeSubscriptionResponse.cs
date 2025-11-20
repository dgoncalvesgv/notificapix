namespace NotificaPix.Core.Contracts.Responses;

public record StripeSubscriptionResponse(string ClientSecret, string SubscriptionId);
