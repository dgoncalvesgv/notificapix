using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using NotificaPix.Core.Abstractions.Services;

namespace NotificaPix.Infrastructure.Services;

public class WebhookDispatcher(IHttpClientFactory factory, ILogger<WebhookDispatcher> logger) : IWebhookDispatcher
{
    public async Task DispatchAsync(string url, string payload, string signature, CancellationToken cancellationToken)
    {
        var client = factory.CreateClient("webhooks");
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-NotificaPix-Signature", signature);
        var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("Webhook delivery failed ({Status}): {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Webhook failed with {response.StatusCode}");
        }
    }
}
