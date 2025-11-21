using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Abstractions.Services;
using NotificaPix.Core.Contracts.Common;
using NotificaPix.Core.Contracts.Requests;
using NotificaPix.Core.Contracts.Responses;

namespace NotificaPix.Api.Endpoints;

public static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/billing").WithTags("Billing").RequireAuthorization("OrgAdmin");
        group.MapGet("/plans", GetPlansAsync);
        group.MapPost("/checkout-session", CreateCheckoutSessionAsync);
        group.MapPost("/portal", CreatePortalAsync);
        return app;
    }

    private static Ok<ApiResponse<IEnumerable<PlanInfoDto>>> GetPlansAsync(IPlanSettingsProvider planSettingsProvider)
    {
        var plans = planSettingsProvider
            .GetAll()
            .Select(t => new PlanInfoDto(
                t.Plan,
                t.Settings.DisplayName,
                t.Settings.PriceText,
                t.Settings.MonthlyTransactions,
                t.Settings.TeamMembersLimit,
                t.Settings.BankAccountsLimit))
            .ToList();

        return TypedResults.Ok(ApiResponse<IEnumerable<PlanInfoDto>>.Ok(plans));
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Ok<ApiResponse<StripeSubscriptionResponse>>> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request,
        ICurrentUserContext currentUser,
        IStripeService stripeService,
        CancellationToken cancellationToken)
    {
        var response = await stripeService.CreateSubscriptionAsync(currentUser.OrganizationId, request, cancellationToken);
        return TypedResults.Ok(ApiResponse<StripeSubscriptionResponse>.Ok(response));
    }

    [Authorize(Policy = "OrgAdmin")]
    private static async Task<Ok<ApiResponse<BillingSessionResponse>>> CreatePortalAsync(
        ICurrentUserContext currentUser,
        IStripeService stripeService,
        CancellationToken cancellationToken)
    {
        var response = await stripeService.CreatePortalSessionAsync(currentUser.OrganizationId, cancellationToken);
        return TypedResults.Ok(ApiResponse<BillingSessionResponse>.Ok(response));
    }
}
