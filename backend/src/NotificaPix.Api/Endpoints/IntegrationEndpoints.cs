using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace NotificaPix.Api.Endpoints;

public static class IntegrationEndpoints
{
    public static IEndpointRouteBuilder MapIntegrationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/integrations/webhook", ReceiveWebhookAsync).WithTags("Integration");
        return app;
    }

    private static async Task<Ok> ReceiveWebhookAsync(HttpContext context, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("IntegrationWebhook");
        using var reader = new StreamReader(context.Request.Body);
        var payload = await reader.ReadToEndAsync();
        logger.LogInformation("Integration webhook received: {Payload}", payload);
        return TypedResults.Ok();
    }
}
