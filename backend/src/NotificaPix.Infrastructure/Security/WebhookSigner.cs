using System.Security.Cryptography;
using System.Text;
using NotificaPix.Core.Abstractions.Security;

namespace NotificaPix.Infrastructure.Security;

public class WebhookSigner : IWebhookSigner
{
    public string Sign(string secret, string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash);
    }

    public bool Verify(string secret, string payload, string signature) =>
        string.Equals(Sign(secret, payload), signature, StringComparison.OrdinalIgnoreCase);
}
