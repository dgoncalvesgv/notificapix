using NotificaPix.Core.Contracts.Requests;
using NotificaPix.Core.Contracts.Responses;

namespace NotificaPix.Core.Abstractions.Services;

public interface IStripeService
{
    Task<StripeSubscriptionResponse> CreateSubscriptionAsync(Guid organizationId, CreateCheckoutSessionRequest request, CancellationToken cancellationToken);
    Task<BillingSessionResponse> CreatePortalSessionAsync(Guid organizationId, CancellationToken cancellationToken);
    Task HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken);
}
