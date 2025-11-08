namespace NotificaPix.Core.Contracts.Requests;

public record NotificationSettingsRequest(IReadOnlyCollection<string> Emails, string? WebhookUrl, string? WebhookSecret, bool Enabled);
