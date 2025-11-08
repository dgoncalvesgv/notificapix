using Microsoft.Extensions.Logging;
using NotificaPix.Core.Abstractions.Services;

namespace NotificaPix.Infrastructure.Services;

public class FakeEmailSender(ILogger<FakeEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending fake email to {To}: {Subject}\n{Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
