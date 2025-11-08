namespace NotificaPix.Core.Abstractions.Services;

public interface IWebhookDispatcher
{
    Task DispatchAsync(string url, string payload, string signature, CancellationToken cancellationToken);
}
