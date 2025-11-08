namespace NotificaPix.Core.Abstractions.Security;

public interface IWebhookSigner
{
    string Sign(string secret, string payload);
    bool Verify(string secret, string payload, string signature);
}
