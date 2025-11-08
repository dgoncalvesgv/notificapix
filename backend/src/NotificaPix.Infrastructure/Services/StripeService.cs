using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Core.Contracts.Requests;
using NotificaPix.Core.Contracts.Responses;
using NotificaPix.Core.Domain.Enums;
using NotificaPix.Infrastructure.Persistence;
using Stripe;
using Stripe.Checkout;

namespace NotificaPix.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly ILogger<StripeService> _logger;
    private readonly NotificaPixDbContext _context;
    private readonly IConfiguration _configuration;

    public StripeService(ILogger<StripeService> logger, NotificaPixDbContext context, IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _configuration = configuration;
        StripeConfiguration.ApiKey = configuration["STRIPE_API_KEY"];
    }

    public async Task<BillingSessionResponse> CreateCheckoutSessionAsync(Guid organizationId, CreateCheckoutSessionRequest request, CancellationToken cancellationToken)
    {
        var organization = await _context.Organizations.FirstAsync(o => o.Id == organizationId, cancellationToken);
        var successUrl = _configuration["CUSTOMER_PORTAL_RETURN_URL"] ?? "http://localhost:5173/app/billing";
        var priceId = ResolvePriceId(request.Plan);

        if (string.IsNullOrWhiteSpace(StripeConfiguration.ApiKey))
        {
            _logger.LogWarning("Stripe API key not configured. Returning mock checkout URL.");
            return new BillingSessionResponse($"https://stripe.com/mock-checkout/{request.Plan.ToString().ToLowerInvariant()}");
        }

        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            SuccessUrl = successUrl,
            CancelUrl = successUrl,
            ClientReferenceId = organization.Id.ToString(),
            Customer = organization.StripeCustomerId,
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["organizationId"] = organization.Id.ToString(),
                    ["plan"] = request.Plan.ToString()
                }
            },
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Price = priceId,
                    Quantity = 1
                }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);
        return new BillingSessionResponse(session.Url);
    }

    public async Task<BillingSessionResponse> CreatePortalSessionAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var organization = await _context.Organizations.FirstAsync(o => o.Id == organizationId, cancellationToken);
        var returnUrl = _configuration["CUSTOMER_PORTAL_RETURN_URL"] ?? "http://localhost:5173/app/billing";

        if (string.IsNullOrWhiteSpace(StripeConfiguration.ApiKey))
        {
            return new BillingSessionResponse($"https://stripe.com/mock-portal/{organization.Id}");
        }

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = organization.StripeCustomerId,
            ReturnUrl = returnUrl
        };
        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);
        return new BillingSessionResponse(session.Url);
    }

    public async Task HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken)
    {
        var webhookSecret = _configuration["STRIPE_WEBHOOK_SECRET"];
        Event stripeEvent;
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            stripeEvent = EventUtility.ParseEvent(payload);
        }
        else
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signature, webhookSecret);
        }

        _logger.LogInformation("Processing Stripe webhook {Type} ({Id})", stripeEvent.Type, stripeEvent.Id);

        switch (stripeEvent.Type)
        {
            case Events.CustomerSubscriptionCreated:
            case Events.CustomerSubscriptionUpdated:
                var subscription = stripeEvent.Data.Object as Subscription;
                if (subscription is not null)
                {
                    await ApplySubscriptionChangeAsync(subscription, cancellationToken);
                }
                break;
            case Events.CustomerSubscriptionDeleted:
                var deleted = stripeEvent.Data.Object as Subscription;
                if (deleted is not null)
                {
                    await DowngradeOrganizationAsync(deleted, cancellationToken);
                }
                break;
            default:
                _logger.LogDebug("Stripe event {Type} is not handled yet", stripeEvent.Type);
                break;
        }
    }

    private async Task ApplySubscriptionChangeAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        if (!subscription.Metadata.TryGetValue("organizationId", out var organizationIdValue) ||
            !Guid.TryParse(organizationIdValue, out var organizationId))
        {
            _logger.LogWarning("Subscription missing organization metadata");
            return;
        }

        var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);
        if (organization is null)
        {
            _logger.LogWarning("Organization {OrganizationId} not found for Stripe subscription {SubscriptionId}", organizationId, subscription.Id);
            return;
        }

        var plan = ResolvePlanFromSubscription(subscription);
        organization.Plan = plan;
        organization.StripeSubscriptionId = subscription.Id;
        organization.StripeCustomerId = subscription.CustomerId;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task DowngradeOrganizationAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        if (!subscription.Metadata.TryGetValue("organizationId", out var organizationIdValue) ||
            !Guid.TryParse(organizationIdValue, out var organizationId))
        {
            return;
        }

        var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);
        if (organization is null) return;
        organization.Plan = PlanType.Starter;
        organization.StripeSubscriptionId = null;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private PlanType ResolvePlanFromSubscription(Subscription subscription)
    {
        var priceId = subscription.Items?.Data?.FirstOrDefault()?.Price?.Id;
        return ResolvePlanFromPrice(priceId);
    }

    private PlanType ResolvePlanFromPrice(string? priceId)
    {
        if (string.Equals(priceId, _configuration["STRIPE_PRICE_STARTER"], StringComparison.OrdinalIgnoreCase))
        {
            return PlanType.Starter;
        }
        if (string.Equals(priceId, _configuration["STRIPE_PRICE_PRO"], StringComparison.OrdinalIgnoreCase))
        {
            return PlanType.Pro;
        }
        if (string.Equals(priceId, _configuration["STRIPE_PRICE_BUSINESS"], StringComparison.OrdinalIgnoreCase))
        {
            return PlanType.Business;
        }
        return PlanType.Starter;
    }

    private string ResolvePriceId(PlanType plan) =>
        plan switch
        {
            PlanType.Pro => _configuration["STRIPE_PRICE_PRO"] ?? "price_pro_mock",
            PlanType.Business => _configuration["STRIPE_PRICE_BUSINESS"] ?? "price_business_mock",
            _ => _configuration["STRIPE_PRICE_STARTER"] ?? "price_starter_mock"
        };
}
