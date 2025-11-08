using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using NotificaPix.Core.Abstractions.Services;

namespace NotificaPix.Api.Endpoints;

public static class WebhookEndpoints
{
    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/webhooks").WithTags("Webhooks");
        group.MapPost("/stripe", HandleStripeAsync);
        group.MapPost("/openfinance", HandleOpenFinanceAsync);
        return app;
    }

    private static async Task<Results<Ok, BadRequest<string>>> HandleStripeAsync(
        HttpContext context,
        IStripeService stripeService,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(context.Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = context.Request.Headers["Stripe-Signature"].ToString();
        try
        {
            await stripeService.HandleWebhookAsync(payload, signature, cancellationToken);
            return TypedResults.Ok();
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest($"Invalid webhook: {ex.Message}");
        }
    }

    private static Task<Ok> HandleOpenFinanceAsync() =>
        Task.FromResult(TypedResults.Ok()); // TODO: implement provider validation once real provider is wired
}
